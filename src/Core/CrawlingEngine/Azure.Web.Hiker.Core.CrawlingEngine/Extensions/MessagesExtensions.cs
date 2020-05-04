using System.Text;

using Microsoft.Azure.ServiceBus;

using Newtonsoft.Json;

namespace Azure.Web.Hiker.Core.CrawlingEngine.Extensions
{
    public static class MessagesExtensions
    {
        public static TValue GetDeserializedMessage<TValue>(this Message message) where TValue : class
        {
            return JsonConvert.DeserializeObject<TValue>(Encoding.UTF8.GetString(message.Body));
        }
    }
}
