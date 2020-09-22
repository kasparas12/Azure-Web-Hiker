using System.Text;

using Azure.Web.Hiker.Core.Common.QueueClient;

using Microsoft.Azure.ServiceBus;

using Newtonsoft.Json;

namespace Azure.Web.Hiker.Infrastructure.ServiceBusClient.Extensions
{
    public static class MessagesExtensions
    {
        public static TValue GetDeserializedMessage<TValue>(this Message message) where TValue : class, IBaseMessage
        {
            return JsonConvert.DeserializeObject<TValue>(Encoding.UTF8.GetString(message.Body));
        }
    }
}
