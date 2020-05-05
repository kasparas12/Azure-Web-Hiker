using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.Common.Messages;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient.Extensions;

using Microsoft.Azure.ServiceBus;

using ServiceFabric.ServiceBus.Services.Netstd;
using ServiceFabric.ServiceBus.Services.Netstd.CommunicationListeners;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgent.MessageHandlers
{
    public class CrawlingQueueMessageHandler : DefaultServiceBusMessageReceiver
    {
        private readonly StatelessServiceContext _context;

        public CrawlingQueueMessageHandler(
            IServiceBusCommunicationListener communicationListener, StatelessServiceContext context) : base(communicationListener)
        {
            _context = context;
        }

        protected override async Task ReceiveMessageImplAsync(Message message, CancellationToken cancellationToken)
        {
            var frontQueueMessage = message.GetDeserializedMessage<AddNewURLToCrawlingAgentMessage>();
            int iterations = 0;

            while (iterations < 10)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ServiceEventSource.Current.ServiceMessage(_context, "Working-{0}", ++iterations);
                ServiceEventSource.Current.ServiceMessage(_context, "URL from queue: ", frontQueueMessage.NewUrl);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }

}