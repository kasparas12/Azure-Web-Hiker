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
        Task SendMessageToCrawlingFrontQueue<T>(T message) where T : IBaseMessage;

    }
}
