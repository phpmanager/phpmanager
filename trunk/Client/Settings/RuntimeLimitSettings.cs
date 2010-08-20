//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using Microsoft.Web.Management.Server;
using System.ComponentModel;
using Microsoft.Web.Management.Client.Win32;

namespace Web.Management.PHP.Settings
{
    internal sealed class RuntimeLimitSettings : SettingPropertyGridObject
    {

        PropertyBag _bag;

        public RuntimeLimitSettings(ModulePropertiesPage page, PropertyBag bag): base(page)
        {
            Initialize(bag);
        }

        internal void Initialize(PropertyBag bag)
        {
            _bag = bag;
        } 

        [SettingCategory("RuntimeLimitsResourceLimits")]
        [SettingDisplayName("RuntimeLimitsMaxExecutionTime", "max_execution_time")]
        [SettingDescription("RuntimeLimitsMaxExecutionTimeDescription")]
        [DefaultValue(typeof(string), "30")]
        public string MaxExecutionTime
        {
            get
            {
                object o = _bag[RuntimeLimitsGlobals.MaxExecutionTime];
                if (o == null)
                {
                    return "30";
                }

                return (string)o;

            }
            set
            {
                _bag[RuntimeLimitsGlobals.MaxExecutionTime] = value;
            }
        }

        [SettingCategory("RuntimeLimitsResourceLimits")]
        [SettingDisplayName("RuntimeLimitsMaxInputTime", "max_input_time")]
        [SettingDescription("RuntimeLimitsMaxInputTimeDescription")]
        [DefaultValue(typeof(string), "60")]
        public string MaxInputTime
        {
            get
            {
                object o = _bag[RuntimeLimitsGlobals.MaxInputTime];
                if (o == null)
                {
                    return "60";
                }

                return (string)o;
            }
            set
            {
                _bag[RuntimeLimitsGlobals.MaxInputTime] = value;
            }
        }

        [SettingCategory("RuntimeLimitsResourceLimits")]
        [SettingDisplayName("RuntimeLimitsMemoryLimit", "memory_limit")]
        [SettingDescription("RuntimeLimitsMemoryLimitDescription")]
        [DefaultValue(typeof(string), "128M")]
        public string MemoryLimit
        {
            get
            {
                object o = _bag[RuntimeLimitsGlobals.MemoryLimit];
                if (o == null)
                {
                    return "128M";
                }

                return (string)o;

            }
            set
            {
                _bag[RuntimeLimitsGlobals.MemoryLimit] = value;
            }
        }

        [SettingCategory("RuntimeLimitsDataHandling")]
        [SettingDisplayName("RuntimeLimitsPostMaxSize", "post_max_size")]
        [SettingDescription("RuntimeLimitsPostMaxSizeDescription")]
        [DefaultValue(typeof(string), "8M")]
        public string PostMaxSize
        {
            get
            {
                object o = _bag[RuntimeLimitsGlobals.PostMaxSize];
                if (o == null)
                {
                    return "8M";
                }

                return (string)o;

            }
            set
            {
                _bag[RuntimeLimitsGlobals.PostMaxSize] = value;
            }
        }

        [SettingCategory("RuntimeLimitsDataHandling")]
        [SettingDisplayName("RuntimeLimitsUploadMaxFilesize", "upload_max_filesize")]
        [SettingDescription("RuntimeLimitsUploadMaxFilesizeDescription")]
        [DefaultValue(typeof(string), "2M")]
        public string UploadMaxFilesize
        {
            get
            {
                object o = _bag[RuntimeLimitsGlobals.UploadMaxFilesize];
                if (o == null)
                {
                    return "2M";
                }

                return (string)o;

            }
            set
            {
                _bag[RuntimeLimitsGlobals.UploadMaxFilesize] = value;
            }
        }

        [SettingCategory("RuntimeLimitsResourceLimits")]
        [SettingDisplayName("RuntimeLimitsMaxFileUploads", "max_file_uploads")]
        [SettingDescription("RuntimeLimitsMaxFileUploadsDescription")]
        [DefaultValue(typeof(string), "20")]
        public string MaxFileUploads
        {
            get
            {
                object o = _bag[RuntimeLimitsGlobals.MaxFileUploads];
                if (o == null)
                {
                    return "20";
                }

                return (string)o;

            }
            set
            {
                _bag[RuntimeLimitsGlobals.MaxFileUploads] = value;
            }
        }

    }
}
