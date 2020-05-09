using System.Threading.Tasks;

namespace Azure.Web.Hiker.Core.AgentRegistrar
{
    public interface IAgentProcessingQueueCreator
    {
        Task CreateNewProcessingQueueForAgent(string agentHostName);
        Task DeleteProcessingQueueForAgent(string agentHostName);
    }
}
