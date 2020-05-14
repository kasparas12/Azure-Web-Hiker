using System.Threading.Tasks;

namespace Azure.Web.Hiker.Core.CrawlingEngine.Interfaces
{
    public interface IAgentProcessingQueueCreator
    {
        Task CreateNewProcessingQueueForAgent(string agentHostName);
        Task DeleteProcessingQueueForAgent(string agentHostName);
    }
}
