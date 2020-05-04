using System;
using System.Collections.Generic;
using System.Fabric;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.CrawlingAgent.Models;
using Azure.Web.Hiker.Core.DnsResolver.Interfaces;

using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

using Newtonsoft.Json;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgent
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CrawlingAgent : StatelessService
    {
        private readonly IDnsResolver _dnsResolver;
        public CrawlingAgent(StatelessServiceContext context, IDnsResolver dnsResolver)
            : base(context)
        {
            _dnsResolver = dnsResolver;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            var info = JsonConvert.DeserializeObject<CrawlerAgentInitializationData>(Encoding.UTF8.GetString(Context.InitializationData));

            IPAddress ip = null;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var conf = Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
                var host = conf.Settings.Sections["AssignedByCrawlingEngineConfigSection"].Parameters["CrawlingHost"].Value;
                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);
                ServiceEventSource.Current.ServiceMessage(this.Context, info.AssignedHostName);

                if (ip is null)
                {
                    ip = _dnsResolver.ResolveHostIpAddress(info.AssignedHostName);
                }

                ServiceEventSource.Current.ServiceMessage(this.Context, "IP: ", ip);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
