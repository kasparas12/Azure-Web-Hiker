using System.Threading.Tasks;

using Azure.Web.Hiker.Core.AgentRegistrar.Models;
using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;
using Azure.Web.Hiker.Core.Common.QueueClient;
using Azure.Web.Hiker.Core.Common.Settings;
using Azure.Web.Hiker.Core.CrawlingEngine.Interfaces;

namespace Azure.Web.Hiker.Core.AgentRegistrar.Services
{
    public class RenderingAgentService : AgentRegistrarServiceBase, IRenderingAgentService
    {
        public RenderingAgentService(
            IAgentRegistrarRepository repository,
            IGeneralApplicationSettings generalApplicationSettings,
            IAgentProcessingQueueCreator agentProcessingQueueCreator,
            IAgentController agentController,
            IWebCrawlerQueueClient webCrawlerQueueClient,
            ISettingsService settingsService) : base(repository, generalApplicationSettings, agentProcessingQueueCreator, agentController, webCrawlerQueueClient, settingsService)
        {
        }

        protected override int GetNumberOfMaxAgents()
        {
            return _settingsService.GetSettingValue<int>("rendering_agents_limit");
        }

        protected override AgentRegistrarEntry GetAgentName(string hostName)
        {
            var number = _repository.GetNextAgentCounterNumber();
            return new AgentRegistrarEntry($"R{number}", hostName);
        }

        protected override async Task SpawnNewAgent(string agentHost, string agentName)
        {
            string serviceType = "RenderAgentType";
            await _agentController.SpawnNewAgentForHostnameAsync(serviceType, agentHost, agentName);
        }
    }
}
