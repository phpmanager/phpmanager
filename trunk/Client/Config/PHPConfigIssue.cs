//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

namespace Web.Management.PHP.Config
{

    internal class PHPConfigIssue : IRemoteObject
    {
        private object[] _data;
        private const int Size = 5;
        private const int IndexName = 0;
        private const int IndexCurrentValue = 1;
        private const int IndexRecommendeValue = 2;
        private const int IndexIssueDescription = 3;
        private const int IndexRecommendation = 4;

        public PHPConfigIssue()
        {
            _data = new object[Size];
        }

        public PHPConfigIssue(string name, string currentValue, string recommendedValue, string issueDescription, string recommendation)
        {
            _data = new object[Size];
            SettingName = name;
            CurrentValue = currentValue;
            RecommendedValue = recommendedValue;
            IssueDescription = issueDescription;
            Recommendation = recommendation;
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

        public string IssueDescription
        {
            get
            {
                return (string)_data[IndexIssueDescription];
            }
            set
            {
                _data[IndexIssueDescription] = value;
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

        public string Recommendation
        {
            get
            {
                return (string)_data[IndexRecommendation];
            }
            set
            {
                _data[IndexRecommendation] = value;
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
