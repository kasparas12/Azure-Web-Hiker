using System.Text;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.Common.QueueClient;

using Microsoft.Azure.ServiceBus;

using Newtonsoft.Json;

namespace Azure.Web.Hiker.Infrastructure.ServiceBusClient
{
    public class ServiceBusQueueClient : IWebCrawlerQueueClient
    {
        private readonly IServiceBusSettings _serviceBusSettings;
        public ServiceBusQueueClient(IServiceBusSettings serviceBusSettings)
        {
            _serviceBusSettings = serviceBusSettings;
        }

        public async Task SendMessage<T>(T message, string queueName) where T : IBaseMessage
        {
            var queueClient = new QueueClient(_serviceBusSettings.ServiceBusConnectionString, queueName);
            var serializedObject = JsonConvert.SerializeObject(message);

            await queueClient.SendAsync(new Message(Encoding.UTF8.GetBytes(serializedObject)));
        }

        public async Task SendMessageToCrawlingAgentProcessingQueue<T>(T message, string hostName) where T : IBaseMessage
        {
            await SendMessage(message, hostName);
        }

        public async Task SendMessageToCrawlingFrontQueue<T>(T message) where T : IBaseMessage
        {
            await SendMessage(message, _serviceBusSettings.CrawlingFrontQueueName);
        }
    }
}
