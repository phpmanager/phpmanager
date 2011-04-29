using System.Management.Automation;
using Web.Management.PHP.Config;
using Microsoft.Web.Administration;
using System.Collections;

namespace Web.Management.PHP
{
    [Cmdlet(VerbsCommon.Get, "PHPVersions")]
    public class GetPHPVersionsCommand : PSCmdlet
    {
        private string _configurationPath;

        [Parameter(ValueFromPipeline = true, Position = 0)]
        public string ConfigurationPath
        {
            set
            {
                _configurationPath = value;
            }
            get
            {
                return _configurationPath;
            }
        }

        protected override void ProcessRecord()
        {
            using (ServerManager serverManager = new ServerManager())
            {
                ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, _configurationPath);
                PHPConfigHelper configHelper = new PHPConfigHelper(serverManagerWrapper);
                RemoteObjectCollection<PHPVersion> phpVersions = configHelper.GetAllPHPVersions();
                foreach (PHPVersion phpVersion in phpVersions)
                {
                    WriteObject(phpVersion);
                }
            }
        }
    }
}
