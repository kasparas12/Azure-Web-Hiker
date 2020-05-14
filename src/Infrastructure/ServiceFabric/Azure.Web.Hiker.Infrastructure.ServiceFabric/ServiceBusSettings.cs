using System.Fabric;
using System.Fabric.Description;

using Azure.Web.Hiker.Infrastructure.ServiceBusClient;

namespace Azure.Web.Hiker.Infrastructure.ServiceFabric
{
    public class ServiceBusSettings : IServiceBusSettings
    {
        public ServiceBusSettings(StatelessServiceContext context)
        {
            context.CodePackageActivationContext.ConfigurationPackageModifiedEvent += CodePackageActivationContext_ConfigurationPackageModifiedEvent;
            UpdateConfigSettings(context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings);
        }

        private void CodePackageActivationContext_ConfigurationPackageModifiedEvent(object sender, PackageModifiedEventArgs<ConfigurationPackage> e)
        {
            UpdateConfigSettings(e.NewPackage.Settings);
        }

        public string ServiceBusConnectionString { get; set; }
        public string AgentCreateQueue { get; set; }
        public string CWCEControlQueue { get; set; }
        public string RenderingServiceBusConnectionString { get; set; }
        public string RenderingQueue { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CrawlingServiceBusNamespaceName { get; set; }
        public string RenderingServiceBusNamespaceName { get; set; }
        public string SubscriptionId { get; set; }
        public string ResourceGroupName { get; set; }

        private void UpdateConfigSettings(ConfigurationSettings settings)
        {
            var generalSectionParams = settings.Sections["ServiceBusConfigSection"].Parameters;

            ServiceBusConnectionString = generalSectionParams["ServiceBusConnectionString"].Value;
            RenderingServiceBusConnectionString = generalSectionParams["RenderingServiceBusConnectionString"].Value;
            AgentCreateQueue = generalSectionParams["AgentCreateQueue"].Value;
            CWCEControlQueue = generalSectionParams["CWCEControlQueue"].Value;
            RenderingQueue = generalSectionParams["RenderQueue"].Value;
            RenderingServiceBusConnectionString = generalSectionParams["RenderingServiceBusConnectionString"].Value;
            RenderingQueue = generalSectionParams["RenderQueue"].Value;
            TenantId = generalSectionParams["TenantId"].Value;
            ClientId = generalSectionParams["ClientId"].Value;
            ClientSecret = generalSectionParams["ClientSecret"].Value;
            CrawlingServiceBusNamespaceName = generalSectionParams["NamespaceName"].Value;
            RenderingServiceBusNamespaceName = generalSectionParams["RenderingNamespaceName"].Value;
            SubscriptionId = generalSectionParams["SubscriptionId"].Value;
            ResourceGroupName = generalSectionParams["ResourceGroupName"].Value;
        }
    }
}
