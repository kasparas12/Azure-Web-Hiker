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
using Azure.Web.Hiker.Core.CrawlingAgent.RenderingDecisionMaker;
using Azure.Web.Hiker.Core.CrawlingEngine.Interfaces;
using Azure.Web.Hiker.Core.DnsResolver.Interfaces;
using Azure.Web.Hiker.Core.IndexStorage.Interfaces;
using Azure.Web.Hiker.DNSResolver.UbietyResolver;
using Azure.Web.Hiker.Infrastructure.Abot2Crawler;
using Azure.Web.Hiker.Infrastructure.ApplicationInsightsTracker;
using Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable;
using Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable.Config;
using Azure.Web.Hiker.Infrastructure.Persistence.Dapper;
using Azure.Web.Hiker.Infrastructure.Persistence.Dapper.Dapper;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient.QueueCreators;
using Azure.Web.Hiker.Infrastructure.ServiceFabric;
using Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgent.MessageHandlers;
using Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine.Services;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

using Newtonsoft.Json;

using SimpleInjector;
using SimpleInjector.Lifestyles;

using static Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable.Config.PageIndexCloudTable;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgent.Container
{
    public static class ContainerConfig
    {
        public static SimpleInjector.Container CreateContainer(StatelessServiceContext context)
        {
            var container = SetInitialContainerSettings(context);

            ConfigureCrawlingHostData(context, container);
            ConfigureRepositories(container, context);
            ConfigureServiceFabricClient(container);
            ConfigureSettings(container, context);
            ConfigureCrawlingURLFilters(container);
            ConfigureCoreServices(container);
            ConfigureServiceBusQueueClients(container, context);
            ConfigurePolitinessCalculator(container, context);
            ConfigureQueueCreators(container, context);
            ConfigureApplicationInsightsTracker(container, context);
            ConfigureRenderingDecisionMaker(container, context);
            ConfigureCrawlingAgentListeningHandlersAndCommunicationListeners(container, context);
            return container;
        }

        private static SimpleInjector.Container SetInitialContainerSettings(StatelessServiceContext context)
        {
            var container = new SimpleInjector.Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            // Register Service Instance
            container.Register(() =>
                new CrawlingAgent(context, container.GetAllInstances<IAzureServiceBusCommunicationListener>()), Lifestyle.Singleton);

            // Registering service context into container
            container.RegisterInstance(context);

            return container;
        }

        private static void ConfigureRepositories(SimpleInjector.Container container, StatelessServiceContext context)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            string connectionString = configurationPackage.Settings.Sections["AgentRegistrarDatabaseSection"].Parameters["ConnectionString"].Value;
            string storageAccountConnectionString = configurationPackage.Settings.Sections["StorageAccountConfigSection"].Parameters["StorageConnectionString"].Value;

            // Registering SQL database repositories
            container.Register<IAgentRegistrarRepository>(() => new DapperAgentRegistrarRepository(connectionString));
            container.Register<ISearchStringRepository>(() => new DapperSearchStringRepository(connectionString), Lifestyle.Singleton);
            container.Register<ISettingsRepository>(() => new DapperSettingsRepository(connectionString));

            // Registering NoSQL Table Storage database repositories
            var cloudTable = PageIndexCloudTable.SetupPageIndexCloudTable(storageAccountConnectionString).GetAwaiter().GetResult();
            var scriptCloudTable = ScriptStorageCloudTable.SetupPageIndexCloudTable(storageAccountConnectionString).GetAwaiter().GetResult();
            container.Register<IPageIndexStorageRepository>(() => new PageIndexStorageRepository(cloudTable), Lifestyle.Transient);
            container.Register<IScriptRepository>(() => new ScriptStorageRepository(scriptCloudTable), Lifestyle.Transient);
        }

        private static void ConfigureServiceFabricClient(SimpleInjector.Container container)
        {
            container.Register(() => new FabricClient(), Lifestyle.Singleton);
        }

        private static void ConfigureSettings(SimpleInjector.Container container, StatelessServiceContext context)
        {
            container.Register<IServiceBusSettings, ServiceBusSettings>();
            container.Register<IGeneralApplicationSettings>(() => new GeneralApplicationSettings(context));
        }

        private static void ConfigureCrawlingURLFilters(SimpleInjector.Container container)
        {
            container.Collection.Register<IPageLinksFilter>(typeof(AvoidPopularSitesFilter), typeof(LeaveLTDomainsFilter));
        }

        private static void ConfigureCoreServices(SimpleInjector.Container container)
        {
            container.Register<IAgentController, FabricAgentController>();
            container.Register<IPageIndexer, PageIndexer>();
            container.Register<IAgentRegistrarService, AgentRegistrarService>();
            container.Register<ISettingsService, SettingsService>();
            container.Register<IPageCrawler, AbotWebCrawler>();

            ConfigureDnsResolver(container);
        }

        // Configuring ASB queue communication clients
        private static void ConfigureServiceBusQueueClients(SimpleInjector.Container container, StatelessServiceContext context)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            string serviceBusConnectionString = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["ServiceBusConnectionString"].Value;
            string renderingServiceBusConnectionString = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["RenderingServiceBusConnectionString"].Value;

            container.Register<IWebCrawlerQueueClient>(() => new ServiceBusQueueClient(container.GetInstance<IServiceBusSettings>(),
                new ServiceBusConnection(serviceBusConnectionString),
                new ManagementClient(serviceBusConnectionString)), Lifestyle.Singleton);

            container.Register<IRenderQueueClient>(() => new ServiceBusQueueClient(container.GetInstance<IServiceBusSettings>(),
                new ServiceBusConnection(renderingServiceBusConnectionString),
                new ManagementClient(renderingServiceBusConnectionString)), Lifestyle.Singleton);

        }

        private static void ConfigureCrawlingAgentListeningHandlersAndCommunicationListeners(SimpleInjector.Container container, StatelessServiceContext context)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");

            var assignedHostName = JsonConvert.DeserializeObject<CrawlerAgentInitializationData>(Encoding.UTF8.GetString(context.InitializationData));

            container.Register<IMessageHandler, CrawlingQueueMessageHandler>();
            container.Collection.Register<IAzureServiceBusCommunicationListener>(typeof(CrawlingQueueCommunicationListener));
        }

        private static void ConfigureQueueCreators(SimpleInjector.Container container, StatelessServiceContext context)
        {
            container.Register<IAgentProcessingQueueCreator, CrawlingAgentQueueCreator>();
        }

        private static void ConfigurePolitinessCalculator(SimpleInjector.Container container, StatelessServiceContext context)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            string apiUrl = configurationPackage.Settings.Sections["GeneralApplicationConfigSection"].Parameters["DomainAuthorityDeterminerAPIURL"].Value;
            string apiKey = configurationPackage.Settings.Sections["GeneralApplicationConfigSection"].Parameters["DomainAuthorityDeterminerAPIKey"].Value;

            container.Register<IRobotsParser, TurnerRobotsParser>();
            container.Register<IDomainImportanceCalculator>(() => new OpenRankDomainImportanceCalculator(apiUrl, apiKey));
            container.Register<IPolitenessDeterminer, DefaultPolitenessDeterminer>();
        }

        private static void ConfigureDnsResolver(SimpleInjector.Container container)
        {
            container.Register<IDnsResolver>(() =>
                new UbietyDnsResolver(container.GetInstance<IDnsConfigureSettings>()), Lifestyle.Singleton);
        }

        private static void ConfigureCrawlingHostData(StatelessServiceContext context, SimpleInjector.Container container)
        {
            var assignedHostName = JsonConvert.DeserializeObject<CrawlerAgentInitializationData>(Encoding.UTF8.GetString(context.InitializationData));
            container.Register<ICrawlingAgentHost>(() => new CrawlerAgentInitializationData(assignedHostName.AssignedHostName));
        }

        private static void ConfigureApplicationInsightsTracker(SimpleInjector.Container container, StatelessServiceContext context)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            string instrumentationKey = configurationPackage.Settings.Sections["ApplicationInsightsConfigSection"].Parameters["InstrumentationKey"].Value;

            var telemetryClient = new TelemetryClient(new TelemetryConfiguration(instrumentationKey));
            container.Register<IHttpVisitMetricTracker>(() => new ApplicationInsightsMetricTracker(telemetryClient), Lifestyle.Singleton);
        }

        private static void ConfigureRenderingDecisionMaker(SimpleInjector.Container container, StatelessServiceContext context)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            string instrumentationKey = configurationPackage.Settings.Sections["ApplicationInsightsConfigSection"].Parameters["InstrumentationKey"].Value;

            container.Register<IHtmlScriptsParser, AngleSharpHtmlParser>();
            container.Register<ICheckSumCalculator, MD5ChecksumCalculator>();
            container.Register<IStringSearcher, StringSearcher>(Lifestyle.Singleton);
            container.Register<IRenderDecisionMaker, RenderDecisionMaker>();
        }
    }
}
