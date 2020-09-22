using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;
using Azure.Web.Hiker.Core.AgentRegistrar.Services;
using Azure.Web.Hiker.Core.Common.Extensions;
using Azure.Web.Hiker.Core.Common.QueueClient;
using Azure.Web.Hiker.Core.RenderingAgent.Messages;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient.Extensions;

using Microsoft.Azure.ServiceBus;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine.MessageHandlers
{
    public class RenderingQueueMessageHandler : IMessageHandler
    {
        ISettingsService _settingsService;
        IRenderQueueClient _webCrawlerQueueClient;
        IRenderingAgentRepository _renderingAgentRepository;
        IRenderingAgentService _renderingAgentService;
        public RenderingQueueMessageHandler(ISettingsService settingsService, IRenderQueueClient webCrawlerQueueClient, IRenderingAgentRepository renderingAgentRepository, IRenderingAgentService renderingAgentService)
        {
            _settingsService = settingsService;
            _webCrawlerQueueClient = webCrawlerQueueClient;
            _renderingAgentRepository = renderingAgentRepository;
            _renderingAgentService = renderingAgentService;
        }

        public async Task ReceiveMessageAsync(Message message, CancellationToken cancellationToken)
        {
            var renderMessage = message.GetDeserializedMessage<RenderingQueueMessage>();

            var renderingEnabled = _settingsService.GetSettingValue<bool>("rendering_enabled");

            if (!renderingEnabled)
            {
                await Task.Delay(60000);
                await _webCrawlerQueueClient.SendMessageToRenderingQueue(new RenderingQueueMessage(renderMessage.UrlToRender));
                return;
            }

            if (_renderingAgentService.AgentExistsForGivenHostName(renderMessage.UrlToRender))
            {
                await _webCrawlerQueueClient.SendMessageToCrawlingAgentProcessingQueue(renderMessage, renderMessage.UrlToRender.GetHostOfUrl());
            }
            else
            {
                await TryToCreateNewAgentForUrlAsync(renderMessage.UrlToRender);
            }
        }
        private async Task TryToCreateNewAgentForUrlAsync(string url)
        {
            await _renderingAgentService.CreateNewAgentForHostName(url.GetHostOfUrl());
            await _webCrawlerQueueClient.SendMessageToCrawlingAgentProcessingQueue(new RenderingQueueMessage(url), url.GetHostOfUrl());
        }
    }
}
