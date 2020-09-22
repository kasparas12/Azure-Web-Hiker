using System.Fabric;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.CrawlingAgent.Models;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient;
using Azure.Web.Hiker.Infrastructure.ServiceFabric;

using Microsoft.Azure.ServiceBus;

using Newtonsoft.Json;

using RenderAgent.Handlers;

namespace RenderAgent
{
    public class RenderingAgentQueueProcessingListener : IAzureServiceBusCommunicationListener
    {
        private readonly IServiceBusSettings _serviceBusSettings;
        private QueueClient _queueClient;
        private RenderingAgentQueueProcessingHandler _handler;
        private StatelessServiceContext _context;
        public RenderingAgentQueueProcessingListener(IServiceBusSettings serviceBusSettings, RenderingAgentQueueProcessingHandler handler, StatelessServiceContext context)
        {
            _serviceBusSettings = serviceBusSettings;
            _handler = handler;
            _context = context;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var sbConnectionString = _serviceBusSettings.RenderingServiceBusConnectionString;
            var assignedHostName = JsonConvert.DeserializeObject<CrawlerAgentInitializationData>(Encoding.UTF8.GetString(_context.InitializationData));


            _queueClient = new QueueClient(sbConnectionString, assignedHostName.AssignedHostName);
            _queueClient.RegisterMessageHandler(_handler.ReceiveMessageAsync,
                new MessageHandlerOptions(LogException)
                { MaxConcurrentCalls = 1, AutoComplete = true });

            return Task.FromResult(string.Empty);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            Stop();

            return Task.CompletedTask;
        }

        public void Abort()
        {
            Stop();
        }

        private void Stop()
        {
            _queueClient?.CloseAsync().GetAwaiter().GetResult();
        }

        private Task LogException(ExceptionReceivedEventArgs args)
        {
            return Task.CompletedTask;
        }
    }

}
