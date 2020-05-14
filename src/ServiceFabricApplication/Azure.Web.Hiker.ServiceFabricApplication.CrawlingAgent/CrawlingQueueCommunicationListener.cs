using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Infrastructure.ServiceBusClient;
using Azure.Web.Hiker.Infrastructure.ServiceFabric;

using Microsoft.Azure.ServiceBus;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgent
{
    public class CrawlingQueueCommunicationListener : IAzureServiceBusCommunicationListener
    {
        private readonly IServiceBusSettings _serviceBusSettings;
        private readonly IMessageHandler _crawlingQueueMessageHandler;
        private readonly string _crawlerHost;

        private QueueClient _queueClient;

        public CrawlingQueueCommunicationListener(IServiceBusSettings serviceBusSettings, IMessageHandler crawlingQueueMessageHandler, string crawlerHost)
        {
            _serviceBusSettings = serviceBusSettings;
            _crawlingQueueMessageHandler = crawlingQueueMessageHandler;
            _crawlerHost = crawlerHost;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var sbConnectionString = _serviceBusSettings.ServiceBusConnectionString;
            var queueName = _crawlerHost;

            _queueClient = new QueueClient(sbConnectionString, queueName);
            _queueClient.RegisterMessageHandler(_crawlingQueueMessageHandler.ReceiveMessageAsync,
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
