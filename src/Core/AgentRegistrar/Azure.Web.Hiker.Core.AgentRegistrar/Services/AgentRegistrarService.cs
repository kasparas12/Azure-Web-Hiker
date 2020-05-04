using Azure.Web.Hiker.Core.AgentRegistrar.Models;
using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;

namespace Azure.Web.Hiker.Core.AgentRegistrar.Services
{
    public interface IAgentRegistrarService
    {
        bool AgentExistsForGivenHostName(string hostName);

        IAgentRegistrarEntry CreateNewAgentRegistrarForHostName(string hostname);
    }

    public class AgentRegistrarService : IAgentRegistrarService
    {
        private readonly IAgentRegistrarRepository _repository;

        public AgentRegistrarService(IAgentRegistrarRepository repository)
        {
            _repository = repository;
        }

        public bool AgentExistsForGivenHostName(string hostName)
        {
            return _repository.AgentForSpecificHostExists(hostName);
        }

        public IAgentRegistrarEntry CreateNewAgentRegistrarForHostName(string hostname)
        {
            var newAgentNumber = _repository.GetNextAgentCounterNumber();
            var newEntry = new AgentRegistrarEntry($"A{newAgentNumber}", hostname);

            _repository.InsertNewAgent(newEntry);

            return newEntry;
        }
    }
}
