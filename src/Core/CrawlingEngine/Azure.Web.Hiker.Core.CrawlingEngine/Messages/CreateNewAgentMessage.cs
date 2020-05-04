using System;

using Azure.Web.Hiker.Core.Common.QueueClient;

using Newtonsoft.Json;

namespace Azure.Web.Hiker.Core.CrawlingEngine.Messages
{
    public class CreateNewAgentMessage : IBaseMessage
    {
        public CreateNewAgentMessage()
        {

        }

        public CreateNewAgentMessage(string url)
        {
            NewUrl = url;
        }

        [JsonProperty("newUrl")]
        public string NewUrl { get; set; }

        public string GetHostOfPage()
        {
            var hostUri = new Uri(NewUrl);
            return hostUri.Host;
        }
    }
}
