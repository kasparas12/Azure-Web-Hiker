using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;

using ServiceFabric.ServiceBus.Services.Netstd;
using ServiceFabric.ServiceBus.Services.Netstd.CommunicationListeners;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine
{
    public class ServiceBusMessageReceiverHandler : DefaultServiceBusMessageReceiver
    {
        public ServiceBusMessageReceiverHandler(IServiceBusCommunicationListener communicationListener) : base(communicationListener) { }

        protected override async Task ReceiveMessageImplAsync(Message message, CancellationToken cancellationToken)
        {
            await Console.Out.WriteLineAsync("MESSAGAS:" + message.Body);
        }
    }
}
