//------------------------------------------------------------------------------
// <copyright file="PHPIniFile.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Web.Management.PHP.Config
{

    internal sealed class PHPIniFile : IRemoteObject
    {
        private object[] _data;
        private const int IndexDirectives = 0;
        private const int IndexExtensions = 1;
        private const int Size = 2;

        private RemoteObjectCollection<PHPIniSetting> _settings;
        private RemoteObjectCollection<PHPIniExtension> _extensions;

        private List<PHPIniBase> _rawEntries;
        private string _filename;

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
            get
            {
                if (_extensions == null)
                {
                    _extensions = new RemoteObjectCollection<PHPIniExtension>((ArrayList)_data[IndexExtensions]);
                }

                return _extensions;
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
            get
            {
                if (_rawEntries == null)
                {
                    _rawEntries = new List<PHPIniBase>();
                }

                return _rawEntries;
            }
        }

        public RemoteObjectCollection<PHPIniSetting> Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new RemoteObjectCollection<PHPIniSetting>((ArrayList)_data[IndexDirectives]);
                }

                return _settings;
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
                    int startindex = tmp.IndexOf('[');
                    int endindex = tmp.IndexOf(']');
                    if ((startindex >= 0) && (endindex > startindex))
                    {
                        string name = tmp.Substring(startindex + 1, endindex - startindex - 1);
                        section = name;
                        yield return new PHPIniSection(line);
                    }
                }
                // Process an extension
                else if (tmp.StartsWith("extension=", StringComparison.OrdinalIgnoreCase))
                {
                    string[] split = tmp.Split(new Char[] { '=' });
                    if (split.Length == 2)
                    {
                        string fileName = split[1].Trim();
                        yield return new PHPIniExtension(fileName, true, line);
                    }
                    else
                    {
                        // If extension file is not specified then treat the line as unusable
                        yield return new PHPIniString(line);
                    }
                }
                // Process a directive
                else if (!String.IsNullOrEmpty(tmp))
                {
                    string[] split = tmp.Split(new Char[] { '=' });
                    if (split.Length > 1)
                        yield return new PHPIniSetting(split[0].Trim(), split[1].Trim(new char[] { ' ', '"' }), section, line);
                    else
                        yield return new PHPIniSetting(split[0].Trim(), String.Empty, section, line);
                }
                else
                {
                    // Return empty comment by default
                    yield return new PHPIniString(line);
                }

                line = reader.ReadLine();
            }
        }

        private void AddAllAvailableExtensions(string extensionDir)
        {
            DirectoryInfo di = new DirectoryInfo(extensionDir);
            FileInfo[] files = di.GetFiles("php*.dll");

            int extensionCount = Extensions.Count;
            foreach (FileInfo file in files)
            {
                bool found = false;
                for (int i = 0; i < extensionCount; i++)
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
            foreach (PHPIniSetting setting in settings)
            {
                bool settingFound = false;
                int index = -1;
                int lastIndex = -1;

                for (int i = 0; i < RawEntries.Count; i++)
                {
                    PHPIniBase b = RawEntries[i];

                    PHPIniSetting existing = b as PHPIniSetting;
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

                        if (String.Equals(existing.Section, setting.Section, StringComparison.OrdinalIgnoreCase))
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
                _data[IndexDirectives] = _settings.GetData();
            }
            if (_extensions != null)
            {
                _data[IndexExtensions] = _extensions.GetData();
            }

            return _data;
        }

        private static string GetExtensionSection(string extensionName)
        {
            string sectionName = Path.GetFileNameWithoutExtension(extensionName).ToUpper();
            return '[' + sectionName + ']';
        }

        internal void Parse()
        {
            if (String.IsNullOrEmpty(FileName))
            {
                throw new InvalidOperationException();
            }

            string extensionDir = String.Empty;

            using (StreamReader reader = new StreamReader(FileName))
            {
                foreach (object o in PHPIniFile.ParseIniFile(reader))
                {
                    PHPIniSetting directive = o as PHPIniSetting;
                    if (directive != null)
                    {
                        Settings.Add(directive);
                        RawEntries.Add(directive);

                        // Get the path to the extension directory - this will be used later
                        if (String.Equals(directive.Name, "extension_dir", StringComparison.OrdinalIgnoreCase))
                        {
                            extensionDir = directive.Value;
                        }
                    }
                    else
                    {
                        PHPIniExtension extension = o as PHPIniExtension;
                        if (extension != null)
                        {
                            Extensions.Add(extension);
                            RawEntries.Add(extension);
                        }
                        else
                        {
                            PHPIniBase entry = o as PHPIniBase;
                            if (entry != null)
                            {
                                RawEntries.Add(entry);
                            }
                        }
                    }
                }
            }

            if (String.IsNullOrEmpty(extensionDir) ||
                String.Equals(extensionDir, "./"))
            {
                extensionDir = Path.Combine(Path.GetDirectoryName(FileName), "ext");
            }
            AddAllAvailableExtensions(extensionDir);
        }

        internal bool Remove(PHPIniBase entry)
        {
            return RawEntries.Remove(entry);
        }

        internal void Save(string filename)
        {
            if (String.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (PHPIniBase entry in RawEntries)
                {
                    writer.WriteLine(entry.Text);
                }
            }
        }

        public void SetData(object o)
        {
            _data = (object[])o;
        }

        internal void UpdateExtensions(IEnumerable<PHPIniExtension> extensions)
        {
            foreach (PHPIniExtension extension in extensions)
            {
                int foundIndex = -1;

                for (int i = 0; i < RawEntries.Count; i++)
                {
                    PHPIniBase b = RawEntries[i];

                    PHPIniExtension existing = b as PHPIniExtension;
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
                        PHPIniBase extensionLine = RawEntries[foundIndex];
                        // ... remove the extension section name if it exists
                        if (foundIndex > 0 &&
                            String.Equals(RawEntries[foundIndex - 1].Text, GetExtensionSection(extension.Name), StringComparison.OrdinalIgnoreCase))
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
                        int lastIndex = RawEntries.Count - 1;
                        lastIndex++;
                        RawEntries.Insert(lastIndex, new PHPIniString(GetExtensionSection(extension.Name)));
                        lastIndex++;
                        RawEntries.Insert(lastIndex, extension);
                    }
                }
            }
        }

        #region IRemoteObject Members

        #endregion
    }

    internal abstract class PHPIniBase
    {
        private string _text;

        protected PHPIniBase() {
             }

        protected PHPIniBase(string text)
        {
            _text = text;
        }

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
            }
        }
    }

    internal class PHPIniString : PHPIniBase
    {

        public PHPIniString() {
             }

        public PHPIniString(string rawText) : base(rawText) {
            }
    }

    internal class PHPIniSetting : PHPIniBase, IRemoteObject
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

        public PHPIniSetting(string name, string value, string section) : this(name, value, section, "") {
            }

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

        public override bool Equals(object obj)
        {
            PHPIniSetting setting = obj as PHPIniSetting;
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
            Text = Name + " = " + Value;
        }
    }

    internal class PHPIniExtension : PHPIniBase, IRemoteObject
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

        public PHPIniExtension(string filename, bool enabled)
        {
            _data = new object[Size];
            Name = filename;
            Enabled = enabled;
        }

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
            Text = "extension=" + Name;
        }

        #region IRemoteObject Members

        #endregion
    }

    internal class PHPIniSection : PHPIniBase
    {

        public PHPIniSection() {
             }

        public PHPIniSection(string rawText) : base(rawText) {
             }
    }
}