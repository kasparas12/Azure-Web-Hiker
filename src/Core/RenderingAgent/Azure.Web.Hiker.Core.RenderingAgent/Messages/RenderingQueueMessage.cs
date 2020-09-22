using Azure.Web.Hiker.Core.Common.QueueClient;

using Newtonsoft.Json;

namespace Azure.Web.Hiker.Core.RenderingAgent.Messages
{
    public class RenderingQueueMessage : IBaseMessage
    {
        public RenderingQueueMessage()
        {

        }

        public RenderingQueueMessage(string url)
        {
            UrlToRender = url;
        }

        [JsonProperty("urlToRender")]
        public string UrlToRender { get; set; }
    }
}
