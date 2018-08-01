//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Web.Management.PHP.Config
{

    public sealed class PHPIniFile : IRemoteObject
    {
        private object[] _data;
        private const int IndexSettings = 0;
        private const int IndexExtensions = 1;
        private const int Size = 2;

        private RemoteObjectCollection<PHPIniSetting> _settings;
        private RemoteObjectCollection<PHPIniExtension> _extensions;

        private List<PHPIniBase> _rawEntries;
        private readonly string _filename;

        public PHPIniFile()
        {
            _data = new object[Size];
        }

        public PHPIniFile(string filename)
            : this()
        {
            _filename = filename;
        }

        public RemoteObjectCollection<PHPIniExtension> Extensions
        {
            get {
                return _extensions ?? (_extensions = new RemoteObjectCollection<PHPIniExtension>((ArrayList) _data[IndexExtensions]));
            }
        }

        public string FileName
        {
            get
            {
                return _filename;
            }
        }

        private IList<PHPIniBase> RawEntries
        {
            get { return _rawEntries ?? (_rawEntries = new List<PHPIniBase>()); }
        }

        public RemoteObjectCollection<PHPIniSetting> Settings
        {
            get {
                return _settings ?? (_settings = new RemoteObjectCollection<PHPIniSetting>((ArrayList) _data[IndexSettings]));
            }
        }

        private void AddAllAvailableExtensions(string extensionDir)
        {
            var di = new DirectoryInfo(extensionDir);
            var files = di.GetFiles("php*.dll");

            var extensionCount = Extensions.Count;
            foreach (var file in files)
            {
                var found = false;
                for (var i = 0; i < extensionCount; i++)
                {
                    if (String.Equals(Extensions[i].Name, file.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Extensions.Add(new PHPIniExtension(file.Name, false));
                }
            }
        }

        internal void AddOrUpdateSettings(IEnumerable<PHPIniSetting> settings)
        {
            foreach (var setting in settings)
            {
                var settingFound = false;
                var index = -1;
                var lastIndex = -1;

                for (var i = 0; i < RawEntries.Count; i++)
                {
                    var b = RawEntries[i];

                    var existing = b as PHPIniSetting;
                    if (existing != null)
                    {
                        lastIndex = i;

                        if (String.Equals(existing.Name, setting.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            existing.Value = setting.Value;
                            existing.UpdateText();
                            settingFound = true;

                            break;
                        }

                        // This finds the index after the last setting for a given section
                        if (String.Equals(existing.Section, setting.Section, StringComparison.OrdinalIgnoreCase))
                        {
                            index = i;
                        }
                    }
                    else
                    {
                        // This finds the index after section declaration,
                        // in case there are no settings defined in that section
                        var section = b as PHPIniSection;
                        if ((section != null) && (String.Equals(section.Name, setting.Section, StringComparison.OrdinalIgnoreCase)))
                        {
                            index = i;
                        }
                    }
                }

                if (!settingFound)
                {
                    setting.UpdateText();

                    if (index == -1)
                    {
                        lastIndex++;
                        RawEntries.Insert(lastIndex, new PHPIniString(""));
                        lastIndex++;
                        RawEntries.Insert(lastIndex, new PHPIniString('[' + setting.Section + ']'));
                        lastIndex++;
                        RawEntries.Insert(lastIndex, setting);
                    }
                    else
                    {
                        RawEntries.Insert(index + 1, setting);
                    }
                }
            }
        }

        public object GetData()
        {
            if (_settings != null)
            {
                _data[IndexSettings] = _settings.GetData();
            }
            if (_extensions != null)
            {
                _data[IndexExtensions] = _extensions.GetData();
            }

            return _data;
        }

        internal int GetEnabledExtensionsCount()
        {
            return Extensions.Count(extension => extension.Enabled);
        }

        private static string GetExtensionSection(string extensionName)
        {
            var extension = Path.GetFileNameWithoutExtension(extensionName);
            if (extension != null)
            {
                var sectionName = extension.ToUpper(CultureInfo.InvariantCulture);
                return '[' + sectionName + ']';
            }

            throw new ArgumentException("Cannot extract extension name from the extention file name.");
        }

        internal PHPIniSetting GetSetting(string name)
        {
            return Settings.FirstOrDefault(setting => String.Equals(setting.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        internal void Parse()
        {
            if (String.IsNullOrEmpty(FileName))
            {
                throw new InvalidOperationException();
            }

            var extensionDir = String.Empty;

            using (var reader = new StreamReader(FileName))
            {
                foreach (var o in ParseIniFile(reader))
                {
                    var directive = o as PHPIniSetting;
                    if (directive != null)
                    {
                        Settings.Add(directive);
                        RawEntries.Add(directive);

                        // Get the path to the extension directory - this will be used later
                        if (String.Equals(directive.Name, "extension_dir", StringComparison.OrdinalIgnoreCase))
                        {
                            extensionDir = directive.GetTrimmedValue();
                        }
                    }
                    else
                    {
                        var extension = o as PHPIniExtension;
                        if (extension != null)
                        {
                            Extensions.Add(extension);
                            RawEntries.Add(extension);
                        }
                        else
                        {
                            var entry = o;
                            if (entry != null)
                            {
                                RawEntries.Add(entry);
                            }
                        }
                    }
                }
            }

            if (String.IsNullOrEmpty(extensionDir) ||
                !Path.IsPathRooted(extensionDir))
            {
                extensionDir = Path.Combine(Path.GetDirectoryName(FileName), "ext");
            }

            if (Directory.Exists(extensionDir))
            {
                AddAllAvailableExtensions(extensionDir);
            }
        }

        private static IEnumerable<PHPIniBase> ParseIniFile(TextReader reader)
        {
            string section = String.Empty;

            string line = reader.ReadLine();
            while (line != null)
            {
                string tmp = line.Trim();

                // Process comments
                if (tmp.StartsWith(";", StringComparison.OrdinalIgnoreCase))
                {
                    yield return new PHPIniString(line);
                }
                // Process section
                else if (tmp.StartsWith("[", StringComparison.OrdinalIgnoreCase))
                {
                    var startindex = tmp.IndexOf('[');
                    var endindex = tmp.IndexOf(']');
                    if ((startindex >= 0) && (endindex > startindex))
                    {
                        string name = tmp.Substring(startindex + 1, endindex - startindex - 1);
                        section = name;
                        yield return new PHPIniSection(name, line);
                    }
                }
                // Process the settings and extensions
                else if (!String.IsNullOrEmpty(tmp))
                {
                    var split = tmp.Split(new[] { '=' }, 2);
                    var name = split[0].Trim();
                    var value = String.Empty;

                    if (split.Length > 1)
                    {
                        value = RemoveInlineComment(split[1].Trim());
                    }
                    else
                    {
                        name = RemoveInlineComment(name);
                    }

                    if (String.Equals(name, "extension", StringComparison.OrdinalIgnoreCase) && !String.IsNullOrEmpty(value))
                    {
                        yield return new PHPIniExtension(value, true, line);
                    }
                    else
                    {
                        yield return new PHPIniSetting(name, value, section, line);
                    }
                }
                else
                {
                    // Return empty comment by default
                    yield return new PHPIniString(line);
                }

                line = reader.ReadLine();
            }
        }

        internal bool Remove(PHPIniBase entry)
        {
            return RawEntries.Remove(entry);
        }

        private static string RemoveInlineComment(string line)
        {
            // Take care of the case when value is wrapped in quotes
            if (line.StartsWith("\"", StringComparison.OrdinalIgnoreCase) &&
                line.EndsWith("\"", StringComparison.OrdinalIgnoreCase))
            {
                return line;
            }

            int commentIndex = line.IndexOf(';');
            if (commentIndex >= 0)
            {
                return line.Substring(0, commentIndex);
            }
            return line;
        }

        internal void Save(string filename)
        {
            if (String.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            using (var writer = new StreamWriter(filename))
            {
                foreach (var entry in RawEntries)
                {
                    writer.WriteLine(entry.GetText());
                }
            }
        }

        public void SetData(object o)
        {
            _data = (object[])o;
        }

        internal void UpdateExtensions(IEnumerable<PHPIniExtension> extensions)
        {
            foreach (var extension in extensions)
            {
                var foundIndex = -1;

                for (var i = 0; i < RawEntries.Count; i++)
                {
                    var b = RawEntries[i];

                    var existing = b as PHPIniExtension;
                    if (existing != null)
                    {
                        if (String.Equals(existing.Name, extension.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            foundIndex = i;
                            break;
                        }
                    }
                }

                // If extension is found...
                if (foundIndex >= 0)
                {
                    // ... and is disabled then...
                    if (!extension.Enabled)
                    {
                        var extensionLine = RawEntries[foundIndex];
                        // ... remove the extension section name if it exists
                        if (foundIndex > 0 &&
                            String.Equals(RawEntries[foundIndex - 1].GetText(), GetExtensionSection(extension.Name), StringComparison.OrdinalIgnoreCase))
                        {
                            RawEntries.Remove(RawEntries[foundIndex - 1]);
                        }

                        // remove the exension
                        RawEntries.Remove(extensionLine);
                    }
                }
                else
                {
                    // Extension is not found
                    if (extension.Enabled)
                    {
                        extension.UpdateText();

                        // Add it at the end of the file along with the extension section name
                        var lastIndex = RawEntries.Count - 1;
                        lastIndex++;
                        RawEntries.Insert(lastIndex, new PHPIniString(GetExtensionSection(extension.Name)));
                        lastIndex++;
                        RawEntries.Insert(lastIndex, extension);
                    }
                }
            }
        }

    }

    public abstract class PHPIniBase
    {
        protected string _text;

        protected PHPIniBase() { }

        protected PHPIniBase(string text)
        {
            _text = text;
        }

        public string GetText()
        {
            return _text;
        }
    }

    public sealed class PHPIniString : PHPIniBase
    {

        public PHPIniString() { }

        public PHPIniString(string rawText) : base(rawText) { }
    }

    public sealed class PHPIniSetting : PHPIniBase, IRemoteObject
    {
        private object[] _data;
        private const int Size = 3;
        private const int IndexName = 0;
        private const int IndexValue = 1;
        private const int IndexSection = 2;

        public PHPIniSetting()
        {
            _data = new object[Size];
        }

        public PHPIniSetting(string name, string value, string section) : this(name, value, section, String.Empty) { }

        public PHPIniSetting(string name, string value, string section, string rawText)
            : base(rawText)
        {
            _data = new object[Size];
            Name = name;
            Value = value;
            Section = section;
        }

        public string Name
        {
            get
            {
                return (string)_data[IndexName];
            }
            set
            {
                _data[IndexName] = value;
            }
        }

        public string Section
        {
            get
            {
                return (string)_data[IndexSection];
            }
            set
            {
                _data[IndexSection] = value;
            }
        }

        public string Value
        {
            get
            {
                return (string)_data[IndexValue];
            }
            set
            {
                _data[IndexValue] = value;
            }
        }

        public string GetTrimmedValue()
        {
            var result = (string)_data[IndexValue];
            return result.Trim(new[] {' ', '"' });
        }

        public override bool Equals(object obj)
        {
            var setting = obj as PHPIniSetting;
            if (setting == null)
            {
                return false;
            }

            return (String.Equals(setting.Name, Name, StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(setting.Value, Value, StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(setting.Section, Section, StringComparison.OrdinalIgnoreCase));
        }

        public object GetData()
        {
            return _data;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Value.GetHashCode() ^ Section.GetHashCode();
        }

        #region IRemoteObject Members

        public void SetData(object o)
        {
            _data = (object[])o;
        }

        #endregion

        internal void UpdateText()
        {
            _text = Name + " = " + Value;
        }
    }

    public sealed class PHPIniExtension : PHPIniBase, IRemoteObject
    {
        private object[] _data;
        private const int Size = 2;
        private const int IndexName = 0;
        private const int IndexEnabled = 1;

        public PHPIniExtension()
        {
            _data = new object[Size];
            Enabled = false;
        }

        public PHPIniExtension(string filename, bool enabled): this(filename, enabled, String.Empty) {  }

        public PHPIniExtension(string filename, bool enabled, string rawText): base(rawText)
        {
            _data = new object[Size];
            Name = filename;
            Enabled = enabled;
        }

        public bool Enabled
        {
            get
            {
                return (bool)_data[IndexEnabled];
            }
            set
            {
                _data[IndexEnabled] = value;
            }
        }

        public string Name
        {
            get
            {
                return (string)_data[IndexName];
            }
            set
            {
                _data[IndexName] = value;
            }
        }

        public object GetData()
        {
            return _data;
        }

        public void SetData(object o)
        {
            _data = (object[])o;
        }

        internal void UpdateText()
        {
            _text = "extension=" + Name;
        }

    }

    internal class PHPIniSection : PHPIniBase
    {
        public PHPIniSection() { }

        public PHPIniSection(string name, string rawText) : base(rawText)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}