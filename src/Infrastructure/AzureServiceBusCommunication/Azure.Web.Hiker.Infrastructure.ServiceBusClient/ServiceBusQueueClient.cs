using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.Common.Messages;
using Azure.Web.Hiker.Core.Common.QueueClient;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient.Extensions;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

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

        public async Task<int> GetNumberOfSameLinkMessagesInCrawlingAgentProcessingQueue<T>(Uri url) where T : AddNewURLToCrawlingAgentMessage
        {
            int hitcount = 0;
            var messageReceiver = new MessageReceiver(_serviceBusSettings.ServiceBusConnectionString, url.Host);

            List<T> listOfUrlsInCrawlerQueue = new List<T>();
            int fetchedUrls = 0;

            do
            {
                var listOfUrlsInQueue = await messageReceiver.PeekAsync(20);
                fetchedUrls = listOfUrlsInCrawlerQueue.Count;

                foreach (var urlInQueue in listOfUrlsInQueue)
                {
                    listOfUrlsInCrawlerQueue.Add(urlInQueue.GetDeserializedMessage<T>());
                }

            } while (fetchedUrls > 0);

            foreach (var fetchedUrl in listOfUrlsInCrawlerQueue)
            {
                if (fetchedUrl.NewUrl == url.AbsoluteUri)
                {
                    hitcount++;
                }
            }

            return hitcount;
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

        public async Task SendMessageToCreateNewAgentQueue<T>(T message) where T : IBaseMessage
        {
            await SendMessage(message, _serviceBusSettings.CreateAgentQueue);
        }

        public async Task SendScheduledMessageToCrawlingAgentProcessingQueue<T>(T message, string hostName, DateTime scheduledTime) where T : IBaseMessage
        {
            var queueClient = new QueueClient(_serviceBusSettings.ServiceBusConnectionString, hostName);
            var serializedObject = JsonConvert.SerializeObject(message);

            await queueClient.ScheduleMessageAsync(new Message(Encoding.UTF8.GetBytes(serializedObject)), scheduledTime);
        }
    }
}
