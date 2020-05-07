using System.Fabric;
using System.Text;

using Azure.Web.Hiker.Core.AgentRegistrar;
using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;
using Azure.Web.Hiker.Core.AgentRegistrar.Services;
using Azure.Web.Hiker.Core.Common.Metrics;
using Azure.Web.Hiker.Core.Common.QueueClient;
using Azure.Web.Hiker.Core.Common.Settings;
using Azure.Web.Hiker.Core.CrawlingAgent.Models;
using Azure.Web.Hiker.Core.CrawlingAgent.PageCrawler;
using Azure.Web.Hiker.Core.CrawlingAgent.PageIndexer;
using Azure.Web.Hiker.Core.DnsResolver.Interfaces;
using Azure.Web.Hiker.Core.IndexStorage.Interfaces;
using Azure.Web.Hiker.DNSResolver.UbietyResolver;
using Azure.Web.Hiker.Infrastructure.Abot2Crawler;
using Azure.Web.Hiker.Infrastructure.ApplicationInsightsTracker;
using Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable;
using Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable.Config;
using Azure.Web.Hiker.Infrastructure.Persistence.Dapper;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient;
using Azure.Web.Hiker.Infrastructure.ServiceFabric;
using Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgent.Helpers;
using Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgent.MessageHandlers;
using Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine.Services;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

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
            ConfigureGeneralApplicationConfig(container, context);
            ConfigureCrawlingHostData(context, container);
            ConfigureServiceBusFrontQueueListener(context, container);
            ConfigureServiceBusCrawlingQueueListener(context, container);
            ConfigureAgentProcessingQueueCreator(container, context);
            ConfigureApplicationInsightsTracker(container, context);

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
            string storageAccountConnectionString = configurationPackage.Settings.Sections["StorageAccountConfigSection"].Parameters["StorageConnectionString"].Value;

            var cloudTable = PageIndexCloudTable.SetupPageIndexCloudTable(storageAccountConnectionString).GetAwaiter().GetResult();
            container.Register<IAgentRegistrarRepository>(() => new DapperAgentRegistrarRepository(connectionString));
            container.Register<IPageIndexStorageRepository>(() => new PageIndexStorageRepository(cloudTable), Lifestyle.Transient);
        }

        private static void ConfigureCoreServices(SimpleInjector.Container container)
        {
            container.Register<FabricClient>(() => new FabricClient(), Lifestyle.Singleton);
            container.Register<IAgentController, FabricAgentController>();
            container.Register<IAgentRegistrarService, AgentRegistrarService>();
            container.Register<IServiceBusSettings, ServiceBusSettings>();
            container.Register<IWebCrawlerQueueClient, ServiceBusQueueClient>();
            container.Register<IPageIndexer, PageIndexer>();
            container.Register<IPageCrawler, AbotWebCrawler>();
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
                cl => new CrawlingQueueMessageHandler(cl, container.GetInstance<IPageIndexer>(), container.GetInstance<IPageCrawler>(), container.GetInstance<ICrawlingAgentHost>()), context, assignedHostName.AssignedHostName, serviceBusConnectionString, serviceBusConnectionString), Lifestyle.Singleton);
        }

        private static void ConfigureCrawlingHostData(StatelessServiceContext context, SimpleInjector.Container container)
        {
            var assignedHostName = JsonConvert.DeserializeObject<CrawlerAgentInitializationData>(Encoding.UTF8.GetString(context.InitializationData));
            container.Register<ICrawlingAgentHost>(() => new CrawlerAgentInitializationData(assignedHostName.AssignedHostName));
        }

        private static void ConfigureGeneralApplicationConfig(SimpleInjector.Container container, StatelessServiceContext context)
        {
            container.Register<IGeneralApplicationSettings>(() => new GeneralApplicationSettings(context));
        }

        private static void ConfigureAgentProcessingQueueCreator(SimpleInjector.Container container, StatelessServiceContext context)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");

            string tenantId = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["TenantId"].Value;
            string clientId = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["ClientId"].Value;
            string clientSecret = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["ClientSecret"].Value;
            string namespaceName = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["NamespaceName"].Value;
            string subscriptionId = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["SubscriptionId"].Value;
            string resourceGroupName = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["ResourceGroupName"].Value;

            var serviceBusCredentials = new ServiceBusCredentials(tenantId, clientId, clientSecret, namespaceName, subscriptionId, resourceGroupName);
            container.Register<IAgentProcessingQueueCreator>(() => new ServiceBusAgentProcessingQueueCreator(serviceBusCredentials));
        }

        private static void ConfigureApplicationInsightsTracker(SimpleInjector.Container container, StatelessServiceContext context)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            string instrumentationKey = configurationPackage.Settings.Sections["ApplicationInsightsConfigSection"].Parameters["InstrumentationKey"].Value;

            var telemetryClient = new TelemetryClient(new TelemetryConfiguration(instrumentationKey));
            container.Register<IHttpVisitMetricTracker>(() => new ApplicationInsightsMetricTracker(telemetryClient));
        }
    }
}
