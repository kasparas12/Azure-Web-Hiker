using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Infrastructure.ServiceBusClient;
using Azure.Web.Hiker.Infrastructure.ServiceFabric;
using Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine.MessageHandlers;

using Microsoft.Azure.ServiceBus;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine
{
    public class RenderingServiceBusCommunicationListener : IAzureServiceBusCommunicationListener
    {
        private readonly IServiceBusSettings _serviceBusSettings;
        private readonly RenderingQueueMessageHandler _renderQueueMessageHandler;

        private QueueClient _queueClient;

        public RenderingServiceBusCommunicationListener(IServiceBusSettings serviceBusSettings, RenderingQueueMessageHandler renderQueueMessageHandler)
        {
            _serviceBusSettings = serviceBusSettings;
            _renderQueueMessageHandler = renderQueueMessageHandler;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var sbConnectionString = _serviceBusSettings.RenderingServiceBusConnectionString;
            var queueName = _serviceBusSettings.RenderingQueue;

            _queueClient = new QueueClient(sbConnectionString, queueName);
            _queueClient.RegisterMessageHandler(_renderQueueMessageHandler.ReceiveMessageAsync,
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
