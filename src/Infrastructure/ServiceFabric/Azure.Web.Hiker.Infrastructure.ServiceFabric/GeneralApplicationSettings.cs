
using System.Fabric;
using System.Fabric.Description;

using Azure.Web.Hiker.Core.Common.Settings;

namespace Azure.Web.Hiker.Infrastructure.ServiceFabric
{
    public class GeneralApplicationSettings : IGeneralApplicationSettings
    {
        public GeneralApplicationSettings(StatelessServiceContext
            context)
        {
            context.CodePackageActivationContext.ConfigurationPackageModifiedEvent += CodePackageActivationContext_ConfigurationPackageModifiedEvent;
            UpdateConfigSettings(context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings);
        }
        private void CodePackageActivationContext_ConfigurationPackageModifiedEvent(object sender, PackageModifiedEventArgs<ConfigurationPackage> e)
        {
            UpdateConfigSettings(e.NewPackage.Settings);
        }
        public int MaxNumberOfAgents { get; set; }
        public int AgentInactivityTimeoutValue { get; set; }
        public string CrawlerUserAgent { get; set; }
        public double MinCrawlDelaySeconds { get; set; }
        public double MaxCrawlDelaySeconds { get; set; }

        private void UpdateConfigSettings(ConfigurationSettings settings)
        {
            var generalSectionParams = settings.Sections["GeneralApplicationConfigSection"].Parameters;

            MaxNumberOfAgents = int.Parse(generalSectionParams["MaxAgentsValue"].Value);
            AgentInactivityTimeoutValue = int.Parse(generalSectionParams["AgentInactivityTimeoutValue"].Value);
            CrawlerUserAgent = generalSectionParams["CrawlerUserAgent"].Value;
            MinCrawlDelaySeconds = double.Parse(generalSectionParams["MinCrawlDelay"].Value);
            MaxCrawlDelaySeconds = double.Parse(generalSectionParams["MaxCrawlDelay"].Value);
        }
    }
}
