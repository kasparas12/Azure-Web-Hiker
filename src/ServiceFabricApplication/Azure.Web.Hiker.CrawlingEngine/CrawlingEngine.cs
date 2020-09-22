using System.Collections.Generic;
using System.Fabric;

using Azure.Web.Hiker.Infrastructure.ServiceFabric;

using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingEngine
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CrawlingEngine : StatelessService
    {
        private readonly IEnumerable<IAzureServiceBusCommunicationListener> _communicationListener;
        public CrawlingEngine(StatelessServiceContext context, IEnumerable<IAzureServiceBusCommunicationListener> communicationListener)
            : base(context)
        {
            _communicationListener = communicationListener;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        /// Program.ApplicationContainer.GetInstance<AgentCreateQueueCommunicationListener>()
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            List<ServiceInstanceListener> listeners = new List<ServiceInstanceListener>();
            listeners.Add(new ServiceInstanceListener((context => Program.ApplicationContainer.GetInstance<CrawlingProcessControlCommunicationListener>()), "CrawlingProcessControlListener"));

            int i = 1;
            foreach (var listener in _communicationListener)
            {
                var name = $"listener{i}";
                listeners.Add(new ServiceInstanceListener((context => listener), name));
                i++;
            }
            return listeners;
        }
    }
}
