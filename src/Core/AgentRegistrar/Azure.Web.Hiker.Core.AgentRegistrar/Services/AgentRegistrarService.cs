
using Azure.Web.Hiker.Core.AgentRegistrar.Models;
using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;
using Azure.Web.Hiker.Core.Common.Settings;

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
        private readonly IGeneralApplicationSettings _generalApplicationSettings;

        public AgentRegistrarService(IAgentRegistrarRepository repository, IGeneralApplicationSettings generalApplicationSettings)
        {
            _repository = repository;
            _generalApplicationSettings = generalApplicationSettings;
        }

        public bool AgentExistsForGivenHostName(string hostName)
        {
            return _repository.AgentForSpecificHostExists(hostName);
        }

        public IAgentRegistrarEntry CreateNewAgentRegistrarForHostName(string hostname)
        {
            var numberOfActiveAgents = _repository.GetNumberOfActiveAgents();
            var maxAgentsSetting = _generalApplicationSettings.MaxNumberOfAgents;

            if (numberOfActiveAgents >= maxAgentsSetting)
            {
                return new AgentRegistrarEntry { SuccessfullyCreated = false };
            }

            var newAgentNumber = _repository.GetNextAgentCounterNumber();
            var newEntry = new AgentRegistrarEntry($"A{newAgentNumber}", hostname);

            _repository.InsertNewAgent(newEntry);

            newEntry.SuccessfullyCreated = true;

            return newEntry;
        }
    }
}
