using System.Fabric;
using System.Text;

using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;
using Azure.Web.Hiker.Core.AgentRegistrar.Services;
using Azure.Web.Hiker.Core.Common.QueueClient;
using Azure.Web.Hiker.Core.CrawlingAgent.Models;
using Azure.Web.Hiker.Core.DnsResolver.Interfaces;
using Azure.Web.Hiker.DNSResolver.UbietyResolver;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient;
using Azure.Web.Hiker.Infrastructure.ServiceFabric;
using Azure.Web.Hiker.PersistenceProviders.Dapper;
using Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgent.Helpers;
using Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgent.MessageHandlers;

using Newtonsoft.Json;

using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgent.Container
{
    public static class ContainerConfig
    {
        public static SimpleInjector.Container CreateContainer(StatelessServiceContext context)
        {
            var container = new SimpleInjector.Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            ConfigureSettings(container, context);
            ConfigureDnsResolver(container);
            ConfigureRepositories(container, context);
            ConfigureCoreServices(container);
            ConfigureCrawlingHostData(context, container);
            ConfigureServiceBusFrontQueueListener(context, container);
            ConfigureServiceBusCrawlingQueueListener(context, container);

            container.RegisterInstance<StatelessServiceContext>(context);
            container.Register<CrawlingAgent>(() => new CrawlingAgent(context), Lifestyle.Singleton);

            return container;
        }

        private static void ConfigureSettings(SimpleInjector.Container container, StatelessServiceContext context)
        {
            container.Register<IDnsConfigureSettings>(() => new DnsConfigSettings(context), Lifestyle.Singleton);
        }

        private static void ConfigureDnsResolver(SimpleInjector.Container container)
        {
            container.Register<IDnsResolver>(() =>
                new UbietyDnsResolver(container.GetInstance<IDnsConfigureSettings>()), Lifestyle.Singleton);
        }

        private static void ConfigureRepositories(SimpleInjector.Container container, StatelessServiceContext context)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            string connectionString = configurationPackage.Settings.Sections["AgentRegistrarDatabaseSection"].Parameters["ConnectionString"].Value;

            container.Register<IAgentRegistrarRepository>(() => new DapperAgentRegistrarRepository(connectionString));
        }

        private static void ConfigureCoreServices(SimpleInjector.Container container)
        {
            container.Register<IAgentRegistrarService, AgentRegistrarService>();
            container.Register<IServiceBusSettings, ServiceBusSettings>();
            container.Register<IWebCrawlerQueueClient, ServiceBusQueueClient>();
        }
        private static void ConfigureServiceBusFrontQueueListener(StatelessServiceContext context, SimpleInjector.Container container)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            string serviceBusQueueName = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["CrawlingFrontQueueName"].Value;
            string serviceBusConnectionString = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["ServiceBusConnectionString"].Value;

            container.Register<FrontQueueCommunicationListener>(() => new FrontQueueCommunicationListener(
                cl => new FrontQueueMessageHandler(cl, container.GetInstance<IAgentRegistrarService>(), container.GetInstance<IWebCrawlerQueueClient>()), context, serviceBusQueueName, serviceBusConnectionString, serviceBusConnectionString), Lifestyle.Singleton);
        }

        private static void ConfigureServiceBusCrawlingQueueListener(StatelessServiceContext context, SimpleInjector.Container container)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            string serviceBusConnectionString = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["ServiceBusConnectionString"].Value;

            var assignedHostName = JsonConvert.DeserializeObject<CrawlerAgentInitializationData>(Encoding.UTF8.GetString(context.InitializationData));

            container.Register<CrawlingQueueCommunicationListener>(() => new CrawlingQueueCommunicationListener(
                cl => new CrawlingQueueMessageHandler(cl, context), context, assignedHostName.AssignedHostName, serviceBusConnectionString, serviceBusConnectionString), Lifestyle.Singleton);
        }

        private static void ConfigureCrawlingHostData(StatelessServiceContext context, SimpleInjector.Container container)
        {
            var assignedHostName = JsonConvert.DeserializeObject<CrawlerAgentInitializationData>(Encoding.UTF8.GetString(context.InitializationData));
            container.Register<ICrawlingAgentHost>(() => new CrawlerAgentInitializationData(assignedHostName.AssignedHostName));
        }
    }
}
