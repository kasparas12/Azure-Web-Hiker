using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine
{
    interface IMessageHandler
    {
        Task ReceiveMessageAsync(Message message, CancellationToken cancellationToken);
    }
}
