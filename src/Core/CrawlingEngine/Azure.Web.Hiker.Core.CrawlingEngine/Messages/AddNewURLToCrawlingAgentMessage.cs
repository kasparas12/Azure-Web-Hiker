using System;

using Azure.Web.Hiker.Core.Common.QueueClient;

using Newtonsoft.Json;

namespace Azure.Web.Hiker.Core.CrawlingEngine.Messages
{
    public class AddNewURLToCrawlingAgentMessage : IBaseMessage
    {
        public AddNewURLToCrawlingAgentMessage()
        {

        }

        public AddNewURLToCrawlingAgentMessage(string url)
        {
            UrlToCrawl = url;
        }

        [JsonProperty("urlToCrawl")]
        public string UrlToCrawl { get; set; }

        public string GetHostOfPage()
        {
            var hostUri = new Uri(UrlToCrawl);
            return hostUri.Host;
        }
    }
}
