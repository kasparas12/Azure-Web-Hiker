using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.Common.QueueClient;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

using Newtonsoft.Json;

namespace Azure.Web.Hiker.Infrastructure.ServiceBusClient
{
    public class ServiceBusQueueClient : IWebCrawlerQueueClient
    {
        private readonly IServiceBusSettings _serviceBusSettings;
        private readonly ServiceBusConnection _serviceBusConnection;
        private readonly ManagementClient _managementClient;

        public ServiceBusQueueClient(IServiceBusSettings serviceBusSettings, ServiceBusConnection serviceBusConnection, ManagementClient managementClient)
        {
            _serviceBusSettings = serviceBusSettings;
            _serviceBusConnection = serviceBusConnection;
            _managementClient = managementClient;
        }

        public async Task<long> GetMessageCountInCrawlerQueue(string hostName)
        {
            var queueInfo = await _managementClient.GetQueueRuntimeInfoAsync(hostName);
            return queueInfo.MessageCount;
        }

        public async Task SendMessage<T>(T message, string queueName) where T : IBaseMessage
        {
            var queueClient = new QueueClient(_serviceBusConnection, queueName, ReceiveMode.PeekLock, null);
            var serializedObject = JsonConvert.SerializeObject(message);

            await queueClient.SendAsync(new Message(Encoding.UTF8.GetBytes(serializedObject)));
        }

        public async Task SendMessages<T>(IEnumerable<T> messages, string queueName) where T : IBaseMessage
        {
            var queueClient = new QueueClient(_serviceBusConnection, queueName, ReceiveMode.PeekLock, null);

            foreach (var msg in messages)
            {
                var serializedObject = JsonConvert.SerializeObject(msg);
                await queueClient.SendAsync(new Message(Encoding.UTF8.GetBytes(serializedObject)));
            }
        }

        public async Task SendMessagesToAgentCreateQueue<T>(IEnumerable<T> messages) where T : IBaseMessage
        {
            await SendMessages(messages, _serviceBusSettings.AgentCreateQueue);
        }

        public async Task SendMessagesToCrawlingAgentProcessingQueue<T>(IEnumerable<T> messages, string hostName) where T : IBaseMessage
        {
            await SendMessages(messages, hostName);
        }

        public async Task SendMessageToAgentCreateQueue<T>(T message) where T : IBaseMessage
        {
            await SendMessage(message, _serviceBusSettings.AgentCreateQueue);
        }

        public async Task SendMessageToCrawlingAgentProcessingQueue<T>(T message, string hostName) where T : IBaseMessage
        {
            await SendMessage(message, hostName);
        }

        public async Task SendScheduledMessageToCrawlingAgentProcessingQueue<T>(T message, string hostName, DateTime scheduledTime) where T : IBaseMessage
        {
            var queueClient = new QueueClient(_serviceBusConnection, hostName, ReceiveMode.PeekLock, null);
            var serializedObject = JsonConvert.SerializeObject(message);

            await queueClient.ScheduleMessageAsync(new Message(Encoding.UTF8.GetBytes(serializedObject)), scheduledTime);
        }
    }
}
