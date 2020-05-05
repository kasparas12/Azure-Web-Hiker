using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.Common.Messages;
using Azure.Web.Hiker.Core.IndexStorage.Interfaces;
using Azure.Web.Hiker.Infrastructure.Persistence.AzureStorageTable.Models;
using Azure.Web.Hiker.Infrastructure.ServiceBusClient.Extensions;

using Microsoft.Azure.ServiceBus;

using ServiceFabric.ServiceBus.Services.Netstd;
using ServiceFabric.ServiceBus.Services.Netstd.CommunicationListeners;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgent.MessageHandlers
{
    public class CrawlingQueueMessageHandler : DefaultServiceBusMessageReceiver
    {
        private readonly StatelessServiceContext _context;
        private readonly IPageIndexStorageRepository<PageIndex> _pageIndexRepo;
        public CrawlingQueueMessageHandler(
            IServiceBusCommunicationListener communicationListener, StatelessServiceContext context, IPageIndexStorageRepository<PageIndex> pageIndexRepo) : base(communicationListener)
        {
            _context = context;
            _pageIndexRepo = pageIndexRepo;
        }

        protected override async Task ReceiveMessageImplAsync(Message message, CancellationToken cancellationToken)
        {
            var frontQueueMessage = message.GetDeserializedMessage<AddNewURLToCrawlingAgentMessage>();

            await _pageIndexRepo.InsertNewPageIndex(new PageIndex(frontQueueMessage.NewUrl, 0, false, DateTime.Now));
        }
    }

}