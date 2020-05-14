using Azure.Web.Hiker.Core.CrawlingEngine.Interfaces;

namespace Azure.Web.Hiker.Infrastructure.ServiceBusClient.QueueCreators
{
    public class RenderAgentQueueCreator : QueueCreatorBase, IRenderAgentProcessingQueueCreator
    {
        public RenderAgentQueueCreator(IServiceBusSettings settings) : base(settings)
        {
        }
        protected override string GetNamespaceName()
        {
            return _settings.RenderingServiceBusNamespaceName;
        }
    }
}
