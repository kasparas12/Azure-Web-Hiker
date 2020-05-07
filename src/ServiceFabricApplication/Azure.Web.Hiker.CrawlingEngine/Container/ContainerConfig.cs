using System.Fabric;

using Azure.Web.Hiker.Core.AgentRegistrar;
using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;
using Azure.Web.Hiker.Core.AgentRegistrar.Services;
using Azure.Web.Hiker.Core.Common.QueueClient;
using Azure.Web.Hiker.Core.Common.Settings;
using Azure.Web.Hiker.Core.CrawlingEngine.Interfaces;
using Azure.Web.Hiker.Core.IndexStorage.Interfaces;
using Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable;
using Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable.Config;
using Azure.Web.Hiker.Infrastructure.Persistence.Dapper;
using Azure.Web.Hiker.Infrastructure.Persistence.Dapper.Dapper;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient;
using Azure.Web.Hiker.Infrastructure.ServiceFabric;
using Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine.Services;

using ServiceFabric.ServiceBus.Services.Netstd.CommunicationListeners;

using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine.Container
{
    public static class ContainerConfig
    {
        public static SimpleInjector.Container CreateContainer(StatelessServiceContext context)
        {
            var container = new SimpleInjector.Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.RegisterInstance<StatelessServiceContext>(context);
            container.Register<CrawlingEngine>(() => new CrawlingEngine(context), Lifestyle.Singleton);

            ConfigureRepositories(container, context);
            ConfigureCoreServices(container);
            ConfigureGeneralApplicationConfig(container, context);
            ConfigureAgentProcessingQueueCreator(container, context);
            ConfigureServiceBusListener(context, container);

            return container;
        }

        private static void ConfigureRepositories(SimpleInjector.Container container, StatelessServiceContext context)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            string connectionString = configurationPackage.Settings.Sections["AgentRegistrarDatabaseSection"].Parameters["ConnectionString"].Value;
            string storageAccountConnectionString = configurationPackage.Settings.Sections["StorageAccountConfigSection"].Parameters["StorageConnectionString"].Value;

            var cloudTable = PageIndexCloudTable.SetupPageIndexCloudTable(storageAccountConnectionString).GetAwaiter().GetResult();
            container.Register<IAgentRegistrarRepository>(() => new DapperAgentRegistrarRepository(connectionString));
            container.Register<ISeedUrlRepository>(() => new DapperSeedUrlRepository(connectionString));
            container.Register<IPageIndexStorageRepository>(() => new PageIndexStorageRepository(cloudTable), Lifestyle.Transient);
        }
        private static void ConfigureCoreServices(SimpleInjector.Container container)
        {
            container.Register<IAgentRegistrarService, AgentRegistrarService>();
            container.Register<FabricClient>(() => new FabricClient(), Lifestyle.Singleton);
            container.Register<IAgentController, FabricAgentController>();
            container.Register<IServiceBusSettings, ServiceBusSettings>();
            container.Register<IWebCrawlerQueueClient, ServiceBusQueueClient>();
        }

        private static void ConfigureServiceBusListener(StatelessServiceContext context, SimpleInjector.Container container)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            string serviceBusQueueName = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["CWCEControlQueue"].Value;
            string serviceBusConnectionString = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["ServiceBusConnectionString"].Value;

            container.Register<ServiceBusQueueCommunicationListener>(() => new ServiceBusQueueCommunicationListener(
                cl => new ServiceBusMessageReceiverHandler(cl, container.GetInstance<IAgentRegistrarService>(), container.GetInstance<ISeedUrlRepository>(), container.GetInstance<IPageIndexStorageRepository>(), container.GetInstance<IWebCrawlerQueueClient>()), context, serviceBusQueueName, serviceBusConnectionString, serviceBusConnectionString), Lifestyle.Singleton);
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

        private static void ConfigureGeneralApplicationConfig(SimpleInjector.Container container, StatelessServiceContext context)
        {
            container.Register<IGeneralApplicationSettings>(() => new GeneralApplicationSettings(context));
        }
    }
}
