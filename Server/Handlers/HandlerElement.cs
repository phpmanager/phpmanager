//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Microsoft.Web.Administration;

namespace Web.Management.PHP.Handlers
{

    public sealed class HandlerElement : ConfigurationElement
    {
        private string _executable;
        private string _arguments;

        public bool AllowPathInfo
        {
            get
            {
                return (bool)base["allowPathInfo"];
            }
            set
            {
                base["allowPathInfo"] = value;
            }
        }

        public string Arguments
        {
            get
            {
                if (_arguments == null)
                {
                    string rawExecutable = SplitScriptProcessor(ScriptProcessor, out _arguments);
                    _executable = Environment.ExpandEnvironmentVariables(rawExecutable);
                }
                return _arguments;
            }
        }

        public string Executable
        {
            get
            {
                if (_executable == null)
                {
                    string rawExecutable = SplitScriptProcessor(ScriptProcessor, out _arguments);
                    _executable = Environment.ExpandEnvironmentVariables(rawExecutable);
                }
                return _executable;
            }
        }

        public string Modules
        {
            get
            {
                return (string)base["modules"];
            }
            set
            {
                base["modules"] = value;
            }
        }

        public string Name
        {
            get
            {
                return (string)base["name"];
            }
            set
            {
                base["name"] = value;
            }
        }

        public string Path
        {
            get
            {
                return (string)base["path"];
            }
            set
            {
                base["path"] = value;
            }
        }

        public string PreCondition
        {
            get
            {
                return (string)base["preCondition"];
            }
            set
            {
                base["preCondition"] = value;
            }
        }

        public RequireAccess RequireAccess
        {
            get
            {
                return (RequireAccess)base["requireAccess"];
            }
            set
            {
                base["requireAccess"] = (int)value;
            }
        }

        public ResourceType ResourceType
        {
            get
            {
                return (ResourceType)base["resourceType"];
            }
            set
            {
                base["resourceType"] = (int)value;
            }
        }

        public string ScriptProcessor
        {
            get
            {
                return (string)base["scriptProcessor"];
            }
            set
            {
                base["scriptProcessor"] = value;
                _executable = null;
                _arguments = null;
            }
        }

        public string Type
        {
            get
            {
                return (string)base["type"];
            }
            set
            {
                base["type"] = value;
            }
        }

        public string Verb
        {
            get
            {
                return (string)base["verb"];
            }
            set
            {
                base["verb"] = value;
            }
        }

        private static string SplitScriptProcessor(string scriptProcessor, out string arguments)
        {
            var s = scriptProcessor.Split(new[] { '|' }, StringSplitOptions.None);
            arguments = s.Length > 1 ? s[1] : String.Empty;
            return s[0];
        }
    }
}
