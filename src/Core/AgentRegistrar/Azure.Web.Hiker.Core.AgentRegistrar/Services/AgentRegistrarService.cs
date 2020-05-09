using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.AgentRegistrar.Exceptions;
using Azure.Web.Hiker.Core.AgentRegistrar.Models;
using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;
using Azure.Web.Hiker.Core.Common.QueueClient;
using Azure.Web.Hiker.Core.Common.Settings;

namespace Azure.Web.Hiker.Core.AgentRegistrar.Services
{
    public interface IAgentRegistrarService
    {
        bool AgentExistsForGivenHostName(string hostName);
        Task<IAgentRegistrarEntry> CreateNewAgentForHostName(string hostname);
        Task<bool> RemoveFinishedWorkAgents();
    }

    public class AgentRegistrarService : IAgentRegistrarService
    {
        private readonly IAgentRegistrarRepository _repository;
        private readonly IAgentProcessingQueueCreator _agentProcessingQueueCreator;
        private readonly IGeneralApplicationSettings _generalApplicationSettings;
        private readonly IAgentController _agentController;
        private readonly IWebCrawlerQueueClient _webCrawlerQueueClient;

        public AgentRegistrarService(
            IAgentRegistrarRepository repository,
            IGeneralApplicationSettings generalApplicationSettings,
            IAgentProcessingQueueCreator agentProcessingQueueCreator,
            IAgentController agentController,
            IWebCrawlerQueueClient webCrawlerQueueClient)
        {
            _repository = repository;
            _generalApplicationSettings = generalApplicationSettings;
            _agentProcessingQueueCreator = agentProcessingQueueCreator;
            _agentController = agentController;
            _webCrawlerQueueClient = webCrawlerQueueClient;
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
                Debug.WriteLine("MAX AGENTS!! Time: " + DateTime.Now);

                var someAgentsRemoved = await RemoveFinishedWorkAgents();

                if (!someAgentsRemoved)
                {
                    await Task.Delay(TimeSpan.FromMinutes(2)).ConfigureAwait(false);
                    throw new MaxAgentsRegisteredException("MAX");
                }
            }

            var newAgentNumber = _repository.GetNextAgentCounterNumber();
            var newEntry = new AgentRegistrarEntry($"A{newAgentNumber}", hostname);

            await _agentProcessingQueueCreator.CreateNewProcessingQueueForAgent(newEntry.AgentHost);
            await _agentController.SpawnNewAgentForHostnameAsync(newEntry.AgentHost, newEntry.AgentName);

            _repository.InsertNewAgent(newEntry);

            return newEntry;
        }

        public async Task<bool> RemoveFinishedWorkAgents()
        {
            int timeoutMinutes = _generalApplicationSettings.AgentInactivityTimeoutValue * -1;
            var timeoutDate = DateTime.UtcNow.AddMinutes(timeoutMinutes);

            var hostsForWhichAgentsAreFree = _repository.GetHostsForWhichAgentsAreFree(timeoutDate);

            if (hostsForWhichAgentsAreFree == null || hostsForWhichAgentsAreFree.Count() == 0)
            {
                return false;
            }

            int successfullyKilledAgents = 0;

            foreach (var host in hostsForWhichAgentsAreFree)
            {
                if (await HostQueueIsEmpty(host.Item1))
                {
                    await _agentController.DeleteAgentForHostnameAsync(host.Item2);
                    await _agentProcessingQueueCreator.DeleteProcessingQueueForAgent(host.Item1);
                    _repository.DeleteAgentEntry(host.Item1);

                    successfullyKilledAgents++;
                }
            }

            if (successfullyKilledAgents > 0)
            {
                return true;
            }

            return false;
        }

        private async Task<bool> HostQueueIsEmpty(string hostname)
        {
            var count = await _webCrawlerQueueClient.GetMessageCountInCrawlerQueue(hostname);
            return count == 0;
        }
    }
}
