using System.Fabric;

using Azure.Web.Hiker.Core.DnsResolver.Interfaces;
using Azure.Web.Hiker.DNSResolver.UbietyResolver;
using Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgent.Helpers;

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

            container.RegisterInstance<StatelessServiceContext>(context);
            container.Register<CrawlingAgent>(() => new CrawlingAgent(context, container.GetInstance<IDnsResolver>()), Lifestyle.Singleton);

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
    }
}
