using Azure.Web.Hiker.Core.CrawlingEngine.Interfaces;

namespace Azure.Web.Hiker.Infrastructure.ServiceBusClient.QueueCreators
{
    public class CrawlingAgentQueueCreator : QueueCreatorBase, IAgentProcessingQueueCreator
    {
        public CrawlingAgentQueueCreator(IServiceBusSettings settings) : base(settings)
        {
        }
        protected override string GetNamespaceName()
        {
            return _settings.CrawlingServiceBusNamespaceName;
        }
    }
}
