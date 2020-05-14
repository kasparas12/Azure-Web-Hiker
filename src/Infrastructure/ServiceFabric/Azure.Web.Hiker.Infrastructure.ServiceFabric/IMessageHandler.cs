using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;

namespace Azure.Web.Hiker.Infrastructure.ServiceFabric
{
    public interface IMessageHandler
    {
        Task ReceiveMessageAsync(Message message, CancellationToken cancellationToken);
    }
}
