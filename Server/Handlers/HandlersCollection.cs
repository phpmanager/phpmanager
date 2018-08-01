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

    public sealed class HandlersCollection : ConfigurationElementCollectionBase<HandlerElement>
    {

        public new HandlerElement this[string name]
        {
            get
            {
                for (var i = 0; (i < Count); i = (i + 1))
                {
                    var element = base[i];
                    if (string.Equals(element.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return element;
                    }
                }
                return null;
            }
        }

        public HandlerElement AddCopy(HandlerElement handler)
        {
            var element = CreateElement();
            CopyAttributes(handler, element);
            return Add(element);
        }

        public HandlerElement AddCopyAt(int index, HandlerElement handler)
        {
            var element = CreateElement();
            CopyAttributes(handler, element);
            return AddAt(index, element);
        }

        private static void CopyAttributes(ConfigurationElement source, ConfigurationElement destination)
        {
            foreach (var attribute in source.Attributes)
            {
                if (!attribute.IsInheritedFromDefaultValue)
                {
                    destination[attribute.Name] = attribute.Value;
                }
            }
        }

        public HandlerElement GetActiveHandler(string path)
        {
            for (var i = 0; i < Count; i++)
            {
                var element = base[i];
                if (String.Equals(path, element.Path, StringComparison.OrdinalIgnoreCase))
                {
                    return element;
                }
            }
            return null;
        }

        public HandlerElement GetHandlerByNameAndPath(string name, string path)
        {
            for (var i = 0; i < Count; i++)
            {
                var element = base[i];
                if (String.Equals(name, element.Name, StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(path, element.Path, StringComparison.OrdinalIgnoreCase))
                {
                    return element;
                }
            }
            return null;
        }

        public HandlerElement GetHandler(string path, string scriptProcessor)
        {
            for (var i = 0; i < Count; i++)
            {
                var element = base[i];
                if (String.Equals(path, element.Path, StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(scriptProcessor, element.ScriptProcessor, StringComparison.OrdinalIgnoreCase))
                {
                    return element;
                }
            }
            return null;
        }

    }
}
