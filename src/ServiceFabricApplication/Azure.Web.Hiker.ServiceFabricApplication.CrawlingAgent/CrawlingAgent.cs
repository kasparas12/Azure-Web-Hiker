using System.Collections.Generic;
using System.Fabric;

using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Azure.Web.Hiker.ServiceFabricApplication.CrawlingAgent
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CrawlingAgent : StatelessService
    {
        public CrawlingAgent(StatelessServiceContext context)
            : base(context)
        {

        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(context => Program.ApplicationContainer.GetInstance<CrawlingQueueCommunicationListener>())
            };
        }
    }
}
