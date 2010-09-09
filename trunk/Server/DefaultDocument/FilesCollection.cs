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

namespace Web.Management.PHP.DefaultDocument
{

    internal class FilesCollection : ConfigurationElementCollectionBase<FileElement>
    {

        public new FileElement this[string value]
        {
            get
            {
                for (int i = 0; (i < this.Count); i = (i + 1))
                {
                    FileElement element = base[i];
                    if ((string.Equals(element.Value, value, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        return element;
                    }
                }
                return null;
            }
        }

        public FileElement AddCopy(FileElement file)
        {
            FileElement element = CreateElement();
            CopyAttributes(file, element);
            return Add(element);
        }

        public FileElement AddCopyAt(int index, FileElement file)
        {
            FileElement element = CreateElement();
            CopyAttributes(file, element);
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
    }
}