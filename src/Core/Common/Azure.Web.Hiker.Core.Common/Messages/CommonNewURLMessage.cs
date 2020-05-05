using System;

using Azure.Web.Hiker.Core.Common.QueueClient;

using Newtonsoft.Json;

namespace Azure.Web.Hiker.Core.Common.Messages
{
    public abstract class CommonNewUrlMessage : IBaseMessage
    {
        public CommonNewUrlMessage()
        {

        }

        public CommonNewUrlMessage(string url)
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
