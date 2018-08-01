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

    public sealed class FilesCollection : ConfigurationElementCollectionBase<FileElement>
    {

        public new FileElement this[string value]
        {
            get
            {
                for (var i = 0; (i < Count); i = (i + 1))
                {
                    var element = base[i];
                    if (string.Equals(element.Value, value, StringComparison.OrdinalIgnoreCase))
                    {
                        return element;
                    }
                }
                return null;
            }
        }

        public FileElement AddCopy(FileElement file)
        {
            var element = CreateElement();
            CopyAttributes(file, element);
            return Add(element);
        }

        public FileElement AddCopyAt(int index, FileElement file)
        {
            var element = CreateElement();
            CopyAttributes(file, element);
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
    }
}