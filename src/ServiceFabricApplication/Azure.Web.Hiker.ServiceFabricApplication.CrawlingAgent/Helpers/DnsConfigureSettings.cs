using System.Fabric;
using System.Fabric.Description;

using Azure.Web.Hiker.Core.DnsResolver.Interfaces;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgent.Helpers
{
    public class DnsConfigSettings : IDnsConfigureSettings
    {
        public DnsConfigSettings(StatelessServiceContext context)
        {
            context.CodePackageActivationContext.ConfigurationPackageModifiedEvent += CodePackageActivationContext_ConfigurationPackageModifiedEvent;
            UpdateConfigSettings(context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings);
        }

        private void CodePackageActivationContext_ConfigurationPackageModifiedEvent(object sender, PackageModifiedEventArgs<ConfigurationPackage> e)
        {
            UpdateConfigSettings(e.NewPackage.Settings);
        }

        public string DnsServerIpAddress { get; set; }

        public int DnsRequestsRetryCount { get; set; }

        public int DnsRequestTimeoutValue { get; set; }

        private void UpdateConfigSettings(ConfigurationSettings settings)
        {
            var generalSectionParams = settings.Sections["DnsSection"].Parameters;
            DnsServerIpAddress = generalSectionParams["DnsServerIpAddress"].Value;
            DnsRequestsRetryCount = int.Parse(generalSectionParams["DnsRequestsRetryCount"].Value);
            DnsRequestTimeoutValue = int.Parse(generalSectionParams["DnsRequestTimeoutValue"].Value);
        }
    }
}
