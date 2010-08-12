//------------------------------------------------------------------------------
// <copyright file="HandlerElement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.Web.Administration;

namespace Web.Management.PHP.Handlers
{

    internal class HandlerElement : ConfigurationElement
    {

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

        //public uint ResponseBufferLimit
        //{
        //    get
        //    {
        //        return (uint)base["responseBufferLimit"];
        //    }
        //    set
        //    {
        //        base["responseBufferLimit"] = value;
        //    }
        //}
    }
}
