using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

using ServiceFabric.ServiceBus.Services.Netstd;
using ServiceFabric.ServiceBus.Services.Netstd.CommunicationListeners;

namespace Azure.Web.Hiker.CrawlingEngine
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CrawlingEngine : StatelessService
    {
        public CrawlingEngine(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {

            // In the configuration file, define connection strings: 
            // "Microsoft.ServiceBus.ConnectionString.Receive"
            // and "Microsoft.ServiceBus.ConnectionString.Send"

            // Also, define a QueueName:
            var configurationPackage = Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            string serviceBusQueueName = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["WebCrawlerURLListeningQueue"].Value;
            string serviceBusSendConnectionString = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["ServiceBusSendConnectionString"].Value;
            string serviceBusReceiveConnectionString = configurationPackage.Settings.Sections["ServiceBusConfigSection"].Parameters["ServiceBusReceiveConnectionString"].Value;

            var listener = new ServiceBusQueueCommunicationListener(
                cl => new ServiceBusReceiverHandler(cl), Context, serviceBusQueueName, serviceBusSendConnectionString, serviceBusReceiveConnectionString);

            yield return new ServiceInstanceListener(context => listener);

        }
    }

    public class ServiceBusReceiverHandler : DefaultServiceBusMessageReceiver
    {
        public ServiceBusReceiverHandler(IServiceBusCommunicationListener communicationListener) : base(communicationListener) { }

        protected override async Task ReceiveMessageImplAsync(Message message, CancellationToken cancellationToken)
        {
            await Console.Out.WriteLineAsync("MESSAGAS:" + message.Body);
        }
    }
}
