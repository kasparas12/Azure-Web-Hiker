using System.Fabric;

using Azure.Web.Hiker.Core.AgentRegistrar;
using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;
using Azure.Web.Hiker.Core.AgentRegistrar.Services;
using Azure.Web.Hiker.Core.Common.QueueClient;
using Azure.Web.Hiker.Core.Common.Settings;
using Azure.Web.Hiker.Core.CrawlingAgent.PageCrawler;
using Azure.Web.Hiker.Core.CrawlingAgent.PageIndexer;
using Azure.Web.Hiker.Core.CrawlingEngine.Interfaces;
using Azure.Web.Hiker.Core.IndexStorage.Interfaces;
using Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable;
using Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable.Config;
using Azure.Web.Hiker.Infrastructure.Persistence.Dapper;
using Azure.Web.Hiker.Infrastructure.Persistence.Dapper.Dapper;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient.QueueCreators;
using Azure.Web.Hiker.Infrastructure.ServiceFabric;
using Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine.MessageHandlers;
using Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine.MessageHandlers.Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine.MessageHandlers;
using Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine.Services;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine.Container
{
    public static class ContainerConfig
    {
        public static SimpleInjector.Container CreateContainer(StatelessServiceContext context)
        {
            var container = SetInitialContainerSettings(context);

            ConfigureRepositories(container, context);
            ConfigureServiceFabricClient(container);
            ConfigureSettings(container, context);
            ConfigureCrawlingURLFilters(container);
            ConfigureCoreServices(container);
            ConfigureServiceBusQueueClients(container, context);
            ConfigurePolitinessCalculator(container, context);
            ConfigureQueueCreators(container, context);

            ConfigureCrawlingEngineListeningHandlersAndCommunicationListeners(container);

            ConfigureCrawlingProcessServiceBusListener(context, container);

            return container;
        }

        private static SimpleInjector.Container SetInitialContainerSettings(StatelessServiceContext context)
        {
            var container = new SimpleInjector.Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            // Register Service Instance
            container.Register(() =>
                new CrawlingEngine(context, container.GetAllInstances<IAzureServiceBusCommunicationListener>()), Lifestyle.Singleton);

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
            container.Register<IRenderingAgentRepository>(() => new DapperRenderingAgentRegistrarRepository(connectionString));
            container.Register<ISettingsRepository>(() => new DapperSettingsRepository(connectionString));

            container.Register<ISeedUrlRepository>(() => new DapperSeedUrlRepository(connectionString));

            // Registering NoSQL Table Storage database repositories
            var cloudTable = PageIndexCloudTable.SetupPageIndexCloudTable(storageAccountConnectionString).GetAwaiter().GetResult();
            container.Register<IPageIndexStorageRepository>(() => new PageIndexStorageRepository(cloudTable), Lifestyle.Transient);
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
            container.Collection.Register<IPageLinksFilter>(typeof(AvoidPopularSitesFilter));
        }

        private static void ConfigureCoreServices(SimpleInjector.Container container)
        {
            container.Register<IAgentController, FabricAgentController>();
            container.Register<IPageIndexer, PageIndexer>();
            container.Register<IAgentRegistrarService, AgentRegistrarService>();
            container.Register<ISettingsService, SettingsService>();

            container.Register<IRenderingAgentService>(() =>
                new RenderingAgentService(container.GetInstance<IRenderingAgentRepository>(),
                container.GetInstance<IGeneralApplicationSettings>(),
                container.GetInstance<IRenderAgentProcessingQueueCreator>(),
                container.GetInstance<IAgentController>(),
                container.GetInstance<IRenderQueueClient>(),
                container.GetInstance<ISettingsService>()));
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

        private static void ConfigureCrawlingProcessServiceBusListener(StatelessServiceContext context, SimpleInjector.Container container)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            string serviceBusQueueName = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["CWCEControlQueue"].Value;
            string serviceBusConnectionString = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["ServiceBusConnectionString"].Value;

            container.Register(() => new CrawlingProcessControlCommunicationListener(
                cl => new CrawlingProcessControlServiceBusMessageReceiverHandler(cl, container.GetInstance<IAgentRegistrarService>(), container.GetInstance<ISeedUrlRepository>(), container.GetInstance<IPageIndexStorageRepository>(), container.GetInstance<IWebCrawlerQueueClient>()), context, serviceBusQueueName, serviceBusConnectionString, serviceBusConnectionString), Lifestyle.Singleton);
        }

        private static void ConfigureCrawlingEngineListeningHandlersAndCommunicationListeners(SimpleInjector.Container container)
        {
            container.Register<AgentCreateMessageHandler>();
            container.Register<RenderingQueueMessageHandler>();
            container.Collection.Register<IAzureServiceBusCommunicationListener>(typeof(ServiceBusCommunicationListener), typeof(RenderingServiceBusCommunicationListener));
        }

        private static void ConfigureQueueCreators(SimpleInjector.Container container, StatelessServiceContext context)
        {
            container.Register<IAgentProcessingQueueCreator, CrawlingAgentQueueCreator>();
            container.Register<IRenderAgentProcessingQueueCreator, RenderAgentQueueCreator>();
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
    }
}
