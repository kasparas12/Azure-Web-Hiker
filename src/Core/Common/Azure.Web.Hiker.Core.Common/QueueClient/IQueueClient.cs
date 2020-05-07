using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.Common.Messages;

namespace Azure.Web.Hiker.Core.Common.QueueClient
{
    public interface IBaseMessage
    {

    }

    public interface IWebCrawlerQueueClient
    {
        Task SendMessage<T>(T message, string queueName) where T : IBaseMessage;
        Task SendMessageToCrawlingAgentProcessingQueue<T>(T message, string hostName) where T : IBaseMessage;
        Task SendScheduledMessageToCrawlingAgentProcessingQueue<T>(T message, string hostName, DateTime scheduledTime) where T : IBaseMessage;

        Task SendMessageToCrawlingFrontQueue<T>(T message) where T : IBaseMessage;
        Task SendMessagesToCrawlingFrontQueue<T>(IEnumerable<T> message) where T : IBaseMessage;
        Task<int> GetNumberOfSameLinkMessagesInCrawlingAgentProcessingQueue<T>(Uri url) where T : AddNewURLToCrawlingAgentMessage;
    }
}
