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

namespace Web.Management.PHP.FastCgi
{

    public sealed class ApplicationElement : ConfigurationElement
    {
        private EnvironmentVariablesCollection _environmentVars;
        private string _fullPath;

        public int ActivityTimeout
        {
            get
            {
                return (int)base["activityTimeout"];
            }
            set
            {
                base["activityTimeout"] = value;
            }
        }

        public string Arguments
        {
            get
            {
                return (string)base["arguments"];
            }
            set
            {
                base["arguments"] = value;
            }
        }

        public EnvironmentVariablesCollection EnvironmentVariables
        {
            get
            {
                if (_environmentVars == null)
                {
                    var environmentVars = GetChildElement("environmentVariables");
                    _environmentVars = (EnvironmentVariablesCollection)environmentVars.GetCollection(typeof(EnvironmentVariablesCollection));
                }
                return _environmentVars;
            }
        }

        public bool FlushNamedPipe
        {
            get
            {
                return (bool)base["flushNamedPipe"];
            }
            set
            {
                base["flushNamedPipe"] = value;
            }
        }

        public string FullPath
        {
            get
            {
                if (string.IsNullOrEmpty(_fullPath))
                {
                    var rawFullPath = (string)base["fullPath"];
                    _fullPath = Environment.ExpandEnvironmentVariables(rawFullPath);
                }
                return _fullPath;
            }
            set
            {
                base["fullPath"] = value;
            }
        }

        public int IdleTimeout
        {
            get
            {
                return (int)base["idleTimeout"];
            }
            set
            {
                base["idleTimeout"] = value;
            }
        }

        public long InstanceMaxRequests
        {
            get
            {
                return (long)base["instanceMaxRequests"];
            }
            set
            {
                base["instanceMaxRequests"] = value;
            }
        }

        public int MaxInstances
        {
            get
            {
                return (int)base["maxInstances"];
            }
            set
            {
                base["maxInstances"] = value;
            }
        }

        public string MonitorChangesTo
        {
            get
            {
                return (string)base["monitorChangesTo"];
            }
            set
            {
                base["monitorChangesTo"] = value;
            }
        }

        public Protocol Protocol
        {
            get
            {
                return ((Protocol)base["protocol"]);
            }
            set
            {
                base["protocol"] = (int)value;
            }
        }

        public int QueueLength
        {
            get
            {
                return (int)base["queueLength"];
            }
            set
            {
                base["queueLength"] = value;
            }
        }

        public int RapidFailsPerMinute
        {
            get
            {
                return (int)base["rapidFailsPerMinute"];
            }
            set
            {
                base["rapidFailsPerMinute"] = value;
            }
        }

        public int RequestTimeout
        {
            get
            {
                return (int)base["requestTimeout"];
            }
            set
            {
                base["requestTimeout"] = value;
            }
        }

        // When FastCGI update is not installed then this property does not exist
        // We need to handle this case and eat the exception
        public int SignalBeforeTerminateSeconds
        {
            get
            {
                var result = 0;
                try
                {
                    result = (int)base["signalBeforeTerminateSeconds"];
                }
                catch
                {
                    // Do nothing here
                }
                return result;
            }
            set
            {
                try
                {
                    base["signalBeforeTerminateSeconds"] = value;
                }
                catch
                {
                    // Do nothing here
                }
            }
        }

        // When FastCGI update is not installed then this property does not exist
        // We need to handle this case and eat the exception
        public StderrMode StderrMode
        {
            get
            {
                var result = StderrMode.IgnoreAndReturn200;
                try
                {
                    result = ((StderrMode)base["stderrMode"]);
                }
                catch
                {
                    // Do nothing here
                }
                return result;
            }
            set
            {
                try
                {
                    base["stderrMode"] = (int)value;
                }
                catch
                {
                    // Do nothing here
                }
            }
        }

        public bool MonitorChangesToExists()
        {
            try
            {
                var o = base["monitorChangesTo"];
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
