﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.AgentRegistrar.Services;
using Azure.Web.Hiker.Core.Common.Messages;
using Azure.Web.Hiker.Core.Common.QueueClient;
using Azure.Web.Hiker.Core.CrawlingEngine.Interfaces;
using Azure.Web.Hiker.Core.CrawlingEngine.Messages;
using Azure.Web.Hiker.Core.IndexStorage.Interfaces;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient.Extensions;

using Microsoft.Azure.ServiceBus;

using ServiceFabric.ServiceBus.Services.Netstd;
using ServiceFabric.ServiceBus.Services.Netstd.CommunicationListeners;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine.MessageHandlers
{
    public class CrawlingProcessControlServiceBusMessageReceiverHandler : DefaultServiceBusMessageReceiver
    {
        private readonly IAgentRegistrarService _agentRegistrarService;
        private readonly ISeedUrlRepository _seedUrlRepository;
        private readonly IPageIndexStorageRepository _pageIndexStorageRepository;
        private readonly IWebCrawlerQueueClient _webCrawlerQueueClient;
        public CrawlingProcessControlServiceBusMessageReceiverHandler(
            IServiceBusCommunicationListener communicationListener,
            IAgentRegistrarService agentRegistrarService,
            ISeedUrlRepository seedUrlRepository,
            IPageIndexStorageRepository pageIndexStorageRepository,
            IWebCrawlerQueueClient webCrawlerQueueClient) : base(communicationListener)
        {
            _agentRegistrarService = agentRegistrarService;
            _seedUrlRepository = seedUrlRepository;
            _pageIndexStorageRepository = pageIndexStorageRepository;
            _webCrawlerQueueClient = webCrawlerQueueClient;
        }

        protected override async Task ReceiveMessageImplAsync(Message message, CancellationToken cancellationToken)
        {
            var controlAction = InvestigateControlAction(message);

            switch (controlAction)
            {
                case ControlAction.StartCrawlProcess:
                    await StartCrawlingProcessAsync();
                    break;
                case ControlAction.StopCrawlProcess:
                    await StopCrawlingProcessAsync();
                    break;
                case ControlAction.StartRenderingProcess:
                    await StartRenderProcessAsync();
                    break;
                default:
                    break;

            }
        }

        private ControlAction InvestigateControlAction(Message message)
        {
            try
            {
                var startMessage = message.GetDeserializedMessage<StartCrawlProcessMessage>();

                if (startMessage != null && startMessage.StartCrawling)
                {
                    return ControlAction.StartCrawlProcess;
                }

                throw new Exception();
            }
            catch (Exception)
            {
                try
                {
                    var stopMessage = message.GetDeserializedMessage<StopCrawlProcessMessage>();

                    if (stopMessage != null && stopMessage.StopCrawling)
                    {
                        return ControlAction.StopCrawlProcess;
                    }
                    throw new Exception();
                }
                catch (Exception)
                {
                    try
                    {
                        var startRenderMessage = message.GetDeserializedMessage<StartRenderProcessMessage>();

                        if (startRenderMessage != null && startRenderMessage.StartRendering)
                        {
                            return ControlAction.StartRenderingProcess;
                        }
                        throw new Exception();

                    }
                    catch (Exception)
                    {
                        throw new Exception();
                    }
                }
            }
        }

        private async Task StartCrawlingProcessAsync()
        {
            var seedUrls = (await _seedUrlRepository.GetListOfSeedUrls()).Select(x => x.UrlAddress);
            var unvisitedUrls = await _pageIndexStorageRepository.FilterUnvisitedLinks(seedUrls);

            var unvisitedUrlMessages = unvisitedUrls.Select(x => new CreateNewAgentForURLMessage(x));
            await _webCrawlerQueueClient.SendMessagesToAgentCreateQueue(unvisitedUrlMessages);
        }

        private async Task StartRenderProcessAsync()
        {
            var seedUrls = (await _seedUrlRepository.GetListOfSeedUrls()).Select(x => x.UrlAddress);
            var unvisitedUrls = await _pageIndexStorageRepository.FilterUnvisitedLinks(seedUrls);

            var unvisitedUrlMessages = unvisitedUrls.Select(x => new CreateNewAgentForURLMessage(x));
            await _webCrawlerQueueClient.SendMessagesToAgentCreateQueue(unvisitedUrlMessages);
        }

        private async Task StopCrawlingProcessAsync()
        {
            await Task.Delay(1);
            throw new NotImplementedException();
        }

        private enum ControlAction
        {
            None = 0,
            StartCrawlProcess = 1,
            StopCrawlProcess = 2,
            StartRenderingProcess = 3
        }
    }
}
