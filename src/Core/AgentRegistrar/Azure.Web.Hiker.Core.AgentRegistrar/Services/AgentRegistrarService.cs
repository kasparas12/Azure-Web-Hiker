
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.AgentRegistrar.Exceptions;
using Azure.Web.Hiker.Core.AgentRegistrar.Models;
using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;
using Azure.Web.Hiker.Core.Common.Settings;

namespace Azure.Web.Hiker.Core.AgentRegistrar.Services
{
    public interface IAgentRegistrarService
    {
        bool AgentExistsForGivenHostName(string hostName);

        Task<IAgentRegistrarEntry> CreateNewAgentForHostName(string hostname);
    }

    public class AgentRegistrarService : IAgentRegistrarService
    {
        private readonly IAgentRegistrarRepository _repository;
        private readonly IAgentProcessingQueueCreator _agentProcessingQueueCreator;
        private readonly IGeneralApplicationSettings _generalApplicationSettings;
        private readonly IAgentController _agentController;

        public AgentRegistrarService(
            IAgentRegistrarRepository repository,
            IGeneralApplicationSettings generalApplicationSettings,
            IAgentProcessingQueueCreator agentProcessingQueueCreator,
            IAgentController agentController)
        {
            _repository = repository;
            _generalApplicationSettings = generalApplicationSettings;
            _agentProcessingQueueCreator = agentProcessingQueueCreator;
            _agentController = agentController;
        }

        public bool AgentExistsForGivenHostName(string hostName)
        {
            return _repository.AgentForSpecificHostExists(hostName);
        }

        public async Task<IAgentRegistrarEntry> CreateNewAgentForHostName(string hostname)
        {
            var numberOfActiveAgents = _repository.GetNumberOfActiveAgents();
            var maxAgentsSetting = _generalApplicationSettings.MaxNumberOfAgents;

            if (numberOfActiveAgents >= maxAgentsSetting)
            {
                await Task.Delay(10000);
                throw new MaxAgentsRegisteredException("Max Number of Agents Already Registered");
            }

            var newAgentNumber = _repository.GetNextAgentCounterNumber();
            var newEntry = new AgentRegistrarEntry($"A{newAgentNumber}", hostname);

            _repository.InsertNewAgent(newEntry);

            await _agentProcessingQueueCreator.CreateNewProcessingQueueForAgent(newEntry.AgentHost);
            await _agentController.SpawnNewAgentForHostnameAsync(newEntry.AgentHost, newEntry.AgentName);

            return newEntry;
        }
    }
}
