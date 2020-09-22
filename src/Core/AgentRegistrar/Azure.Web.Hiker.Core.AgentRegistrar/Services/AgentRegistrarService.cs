using System.Threading.Tasks;

using Azure.Web.Hiker.Core.AgentRegistrar.Models;
using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;
using Azure.Web.Hiker.Core.Common.QueueClient;
using Azure.Web.Hiker.Core.Common.Settings;
using Azure.Web.Hiker.Core.CrawlingEngine.Interfaces;

namespace Azure.Web.Hiker.Core.AgentRegistrar.Services
{
    public interface IAgentRegistrarService
    {
        bool AgentExistsForGivenHostName(string hostName);
        Task<IAgentRegistrarEntry> CreateNewAgentForHostName(string hostname);
        Task<bool> RemoveFinishedWorkAgents();
    }

    public interface IRenderingAgentService : IAgentRegistrarService
    {

    }

    public class AgentRegistrarService : AgentRegistrarServiceBase, IAgentRegistrarService
    {
        public AgentRegistrarService(
            IAgentRegistrarRepository repository,
            IGeneralApplicationSettings generalApplicationSettings,
            IAgentProcessingQueueCreator agentProcessingQueueCreator,
            IAgentController agentController,
            IWebCrawlerQueueClient webCrawlerQueueClient,
            ISettingsService settingsService) : base(repository, generalApplicationSettings, agentProcessingQueueCreator, agentController, webCrawlerQueueClient, settingsService)
        {
        }

        protected override AgentRegistrarEntry GetAgentName(string hostName)
        {
            var number = _repository.GetNextAgentCounterNumber();
            return new AgentRegistrarEntry($"A{number}", hostName);
        }

        protected override int GetNumberOfMaxAgents()
        {
            return _settingsService.GetSettingValue<int>("crawling_agents_limit");
        }

        protected override async Task SpawnNewAgent(string agentHost, string agentName)
        {
            string serviceType = "Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgentType";
            await _agentController.SpawnNewAgentForHostnameAsync(serviceType, agentHost, agentName);
        }
    }
}
