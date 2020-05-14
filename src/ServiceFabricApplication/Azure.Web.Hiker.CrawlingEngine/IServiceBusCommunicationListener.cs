using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Infrastructure.ServiceBusClient;
using Azure.Web.Hiker.Infrastructure.ServiceFabric;
using Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine.MessageHandlers.Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine.MessageHandlers;

using Microsoft.Azure.ServiceBus;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine
{

    public class ServiceBusCommunicationListener : IAzureServiceBusCommunicationListener
    {
        private readonly IServiceBusSettings _serviceBusSettings;
        private readonly AgentCreateMessageHandler _agentCreateMessageHandler;

        private QueueClient _queueClient;

        public ServiceBusCommunicationListener(IServiceBusSettings serviceBusSettings, AgentCreateMessageHandler agentCreateMessageHandler)
        {
            _serviceBusSettings = serviceBusSettings;
            _agentCreateMessageHandler = agentCreateMessageHandler;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var sbConnectionString = _serviceBusSettings.ServiceBusConnectionString;
            var queueName = _serviceBusSettings.AgentCreateQueue;

            _queueClient = new QueueClient(sbConnectionString, queueName);
            _queueClient.RegisterMessageHandler(_agentCreateMessageHandler.ReceiveMessageAsync,
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
