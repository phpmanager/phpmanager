//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;

namespace Web.Management.PHP.Config
{

    internal class PHPConfigIssue : IRemoteObject
    {
        private object[] _data;
        private const int Size = 4;
        private const int IndexName = 0;
        private const int IndexCurrentValue = 1;
        private const int IndexRecommendeValue = 2;
        private const int IndexDescription = 3;

        public PHPConfigIssue()
        {
            _data = new object[Size];
        }

        public PHPConfigIssue(string name, string currentValue, string recommendedValue, string description)
        {
            _data = new object[Size];
            SettingName = name;
            CurrentValue = currentValue;
            RecommendedValue = recommendedValue;
            Description = description;
        }

        public string CurrentValue
        {
            get
            {
                return (string)_data[IndexCurrentValue];
            }
            set
            {
                _data[IndexCurrentValue] = value;
            }
        }

        public string Description
        {
            get
            {
                return (string)_data[IndexDescription];
            }
            set
            {
                _data[IndexDescription] = value;
            }
        }

        public string RecommendedValue
        {
            get
            {
                return (string)_data[IndexRecommendeValue];
            }
            set
            {
                _data[IndexRecommendeValue] = value;
            }
        }

        public string SettingName
        {
            get
            {
                return (string)_data[IndexName];
            }
            set
            {
                _data[IndexName] = value;
            }
        }

        public object GetData()
        {
            return _data;
        }

        public void SetData(object o)
        {
            _data = (object[])o;
        }
    }
}
