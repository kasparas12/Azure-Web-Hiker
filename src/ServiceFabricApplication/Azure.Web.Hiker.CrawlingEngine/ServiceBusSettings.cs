using System.Fabric;
using System.Fabric.Description;

using Azure.Web.Hiker.Infrastructure.ServiceBusClient;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine
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

        public string CrawlingFrontQueueName { get; set; }

        private void UpdateConfigSettings(ConfigurationSettings settings)
        {
            var generalSectionParams = settings.Sections["ServiceBusConfigSection"].Parameters;

            ServiceBusConnectionString = generalSectionParams["ServiceBusConnectionString"].Value;
            CrawlingFrontQueueName = generalSectionParams["CrawlingFrontQueueName"].Value;
        }
    }
}
