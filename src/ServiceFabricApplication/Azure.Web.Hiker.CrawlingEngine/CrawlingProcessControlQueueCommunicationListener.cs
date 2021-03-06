﻿
using System;
using System.Fabric;

using ServiceFabric.ServiceBus.Services.Netstd;
using ServiceFabric.ServiceBus.Services.Netstd.CommunicationListeners;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine
{
    public class CrawlingProcessControlCommunicationListener : ServiceBusQueueCommunicationListener
    {
        public CrawlingProcessControlCommunicationListener(Func<IServiceBusCommunicationListener, IServiceBusMessageReceiver> receiverFactory, ServiceContext context, string serviceBusQueueName, string serviceBusSendConnectionString, string serviceBusReceiveConnectionString)
            : base(receiverFactory, context, serviceBusQueueName, serviceBusSendConnectionString, serviceBusReceiveConnectionString)
        {

        }

    }
}
