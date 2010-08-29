//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using Microsoft.Web.Administration;

namespace Web.Management.PHP.FastCgi
{

    internal class ApplicationElement : ConfigurationElement
    {
        private EnvironmentVariablesCollection _environmentVars;

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
                if (this._environmentVars == null)
                {
                    ConfigurationElement environmentVars = base.GetChildElement("environmentVariables");
                    this._environmentVars = (EnvironmentVariablesCollection)environmentVars.GetCollection(typeof(EnvironmentVariablesCollection));
                }
                return this._environmentVars;
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
                return (string)base["fullPath"];
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

        public int InstanceMaxRequests
        {
            get
            {
                return (int)base["instanceMaxRequests"];
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

        // When FastCGI update is not installed then this property does not exist
        // We need to handle this case and eat the exception
        public string MonitorChangesTo
        {
            get
            {
                string result = String.Empty;
                try
                {
                    result = (string)base["monitorChangesTo"];
                }
                catch
                {
                    // Do nothing here...
                }
                return result; 
            }
            set
            {
                try
                {
                    base["monitorChangesTo"] = value;
                }
                catch
                {
                }
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
                int result = 0;
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
                StderrMode result = StderrMode.IgnoreAndReturn200;
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
    }
}
