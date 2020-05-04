using System.Threading.Tasks;

namespace Azure.Web.Hiker.Core.CrawlingEngine.Services
{
    public interface IAgentProcessingQueueCreator
    {
        Task CreateNewProcessingQueueForAgent(string agentHostName);
    }
}
