//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using Microsoft.Web.Administration;

namespace Web.Management.PHP.DefaultDocument
{

    public sealed class DefaultDocumentSection : ConfigurationSection
    {
        private FilesCollection _files;

        public FilesCollection Files
        {
            get
            {
                if (_files == null)
                {
                    var files = GetChildElement("files");
                    _files = (FilesCollection)files.GetCollection(typeof(FilesCollection));
                }
                return _files;
            }
        }
    }
}
