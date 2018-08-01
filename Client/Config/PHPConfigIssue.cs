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

    public static class PHPConfigIssueIndex
    {
        public const int DefaultDocument = 0;
        public const int ResourceType = 1;
        public const int PhpMaxRequests = 2;
        public const int Phprc = 3;
        public const int MonitorChangesTo = 4;
        public const int ExtensionDir = 5;
        public const int LogErrors = 6;
        public const int ErrorLog = 7;
        public const int SessionPath = 8;
        public const int CgiForceRedirect = 9;
        public const int CgiPathInfo = 10;
        public const int FastCgiImpersonation = 11;
        public const int UploadDir = 12;
        public const int DateTimeZone = 13;
    }

    public class PHPConfigIssue : IRemoteObject
    {
        private object[] _data;
        private const int Size = 6;
        private const int IndexName = 0;
        private const int IndexCurrentValue = 1;
        private const int IndexRecommendeValue = 2;
        private const int IndexIssueDescription = 3;
        private const int IndexRecommendation = 4;
        private const int IndexIssueIndex = 5;

        public PHPConfigIssue()
        {
            _data = new object[Size];
        }

        public PHPConfigIssue(string name, string currentValue, string recommendedValue, string issueDescription, string recommendation, int issueIndex)
        {
            _data = new object[Size];
            SettingName = name;
            CurrentValue = currentValue;
            RecommendedValue = recommendedValue;
            IssueDescription = issueDescription;
            Recommendation = recommendation;
            IssueIndex = issueIndex;
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

        public int IssueIndex
        {
            get
            {
                return (int)_data[IndexIssueIndex];
            }
            set
            {
                _data[IndexIssueIndex] = value;
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
