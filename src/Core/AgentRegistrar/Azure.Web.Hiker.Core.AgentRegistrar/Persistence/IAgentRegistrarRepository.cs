using Azure.Web.Hiker.Core.AgentRegistrar.Models;

namespace Azure.Web.Hiker.Core.AgentRegistrar.Persistence
{
    public interface IAgentRegistrarRepository
    {
        bool AgentForSpecificHostExists(string hostname);
        IAgentRegistrarEntry GetAgentForSpecificHost(string hostname);
        void InsertNewAgent(IAgentRegistrarEntry newEntry);
        void DeleteAgent(int agentId);
        int GetNextAgentCounterNumber();
        int GetNumberOfActiveAgents();
    }
}
