using System;
using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Infrastructure.ServiceFabric;

using Microsoft.Azure.ServiceBus;

namespace RenderAgent.Handlers
{
    public class RenderingAgentQueueProcessingHandler : IMessageHandler
    {
        public Task ReceiveMessageAsync(Message message, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
