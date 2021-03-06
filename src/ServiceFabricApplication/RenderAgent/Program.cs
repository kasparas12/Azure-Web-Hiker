using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;

using Microsoft.ServiceFabric.Services.Runtime;

using RenderAgent.Container;

namespace RenderAgent
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        /// 
        public static SimpleInjector.Container ApplicationContainer { get; set; }

        private static void Main()
        {
            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                ServiceRuntime.RegisterServiceAsync("RenderAgentType",
                    context => CreateService(context));

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(RenderAgent).Name);

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }


        }

        private static StatelessService CreateService(StatelessServiceContext context)
        {
            var container = ContainerConfig.CreateContainer(context);
            ApplicationContainer = container;
            return container.GetInstance<RenderAgent>();
        }
    }
}
