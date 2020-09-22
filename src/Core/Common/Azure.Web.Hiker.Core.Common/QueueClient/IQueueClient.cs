using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.Web.Hiker.Core.Common.QueueClient
{
    public interface IBaseMessage
    {

    }

    public interface IWebCrawlerQueueClient
    {
        Task SendMessage<T>(T message, string queueName) where T : IBaseMessage;
        Task SendMessageToCrawlingAgentProcessingQueue<T>(T message, string hostName) where T : IBaseMessage;
        Task SendMessagesToCrawlingAgentProcessingQueue<T>(IEnumerable<T> message, string hostName) where T : IBaseMessage;

        Task SendScheduledMessageToCrawlingAgentProcessingQueue<T>(T message, string hostName, DateTime scheduledTime) where T : IBaseMessage;

        Task SendMessageToAgentCreateQueue<T>(T message) where T : IBaseMessage;
        Task SendMessageToRenderingQueue<T>(T message) where T : IBaseMessage;
        Task SendMessagesToAgentCreateQueue<T>(IEnumerable<T> message) where T : IBaseMessage;
        Task<long> GetMessageCountInCrawlerQueue(string hostName);
    }

    public interface IRenderQueueClient : IWebCrawlerQueueClient { }
}
