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
                for (int i = 0; (i < this.Count); i = (i + 1))
                {
                    HandlerElement element = base[i];
                    if ((string.Equals(element.Name, name, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        return element;
                    }
                }
                return null;
            }
        }

        public HandlerElement AddCopy(HandlerElement handler)
        {
            HandlerElement element = CreateElement();
            CopyAttributes(handler, element);
            return Add(element);
        }

        public HandlerElement AddCopyAt(int index, HandlerElement handler)
        {
            HandlerElement element = CreateElement();
            CopyAttributes(handler, element);
            return AddAt(index, element);
        }

        private static void CopyAttributes(ConfigurationElement source, ConfigurationElement destination)
        {
            foreach (ConfigurationAttribute attribute in source.Attributes)
            {
                if (!attribute.IsInheritedFromDefaultValue)
                {
                    destination[attribute.Name] = attribute.Value;
                }
            }
        }

        public HandlerElement GetActiveHandler(string path)
        {
            for (int i = 0; i < Count; i++)
            {
                HandlerElement element = base[i];
                if (String.Equals(path, element.Path, StringComparison.OrdinalIgnoreCase))
                {
                    return element;
                }
            }
            return null;
        }

        public HandlerElement GetHandlerByNameAndPath(string name, string path)
        {
            for (int i = 0; i < Count; i++)
            {
                HandlerElement element = base[i];
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
            for (int i = 0; i < Count; i++)
            {
                HandlerElement element = base[i];
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
