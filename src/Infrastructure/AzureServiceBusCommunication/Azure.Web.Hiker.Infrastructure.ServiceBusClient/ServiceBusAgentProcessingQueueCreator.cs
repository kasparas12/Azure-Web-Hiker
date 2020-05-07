using System;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.AgentRegistrar;

using Microsoft.Azure.Management.ServiceBus;
using Microsoft.Azure.Management.ServiceBus.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;

namespace Azure.Web.Hiker.Infrastructure.ServiceBusClient
{
    public class ServiceBusCredentials
    {
        public ServiceBusCredentials(string tenantId, string clientId, string clientSecret, string namespaceName, string subscriptionId, string resourceGroupName)
        {
            TenantId = tenantId;
            ClientId = clientId;
            ClientSecret = clientSecret;
            NamespaceName = namespaceName;
            SubscriptionId = subscriptionId;
            ResourceGroupName = resourceGroupName;
        }

        public string TenantId { get; }
        public string ClientId { get; }
        public string ClientSecret { get; }
        public string NamespaceName { get; }
        public string SubscriptionId { get; }
        public string ResourceGroupName { get; }
    }

    public class ServiceBusAgentProcessingQueueCreator : IAgentProcessingQueueCreator
    {
        private readonly ServiceBusCredentials _serviceBusCredentials;

        private const string authenticationLink = "https://login.microsoftonline.com/";
        private const string azureManagementLink = "https://management.core.windows.net/";

        private DateTime? tokenExpiresAtUtc;
        private string _token;

        public ServiceBusAgentProcessingQueueCreator(ServiceBusCredentials serviceBusCredentials)
        {
            _serviceBusCredentials = serviceBusCredentials;
        }

        public async Task CreateNewProcessingQueueForAgent(string agentHostName)
        {
            if (string.IsNullOrEmpty(_serviceBusCredentials.NamespaceName))
            {
                throw new Exception("Namespace name is empty!");
            }

            var token = await ObtainToken();

            var creds = new TokenCredentials(token);
            var sbClient = new ServiceBusManagementClient(creds)
            {
                SubscriptionId = _serviceBusCredentials.SubscriptionId,
            };

            var queueParams = new SBQueue
            {
                EnablePartitioning = true
            };

            try
            {
                await sbClient.Queues.CreateOrUpdateAsync(_serviceBusCredentials.ResourceGroupName, _serviceBusCredentials.NamespaceName, agentHostName, queueParams);
            }
            catch (Exception e)
            {
                var b = e;
            }
        }

        private async Task<string> ObtainToken()
        {
            // Check to see if the token has expired before requesting one.
            // We will go ahead and request a new one if we are within 2 minutes of the token expiring.
            if (!tokenExpiresAtUtc.HasValue || tokenExpiresAtUtc < DateTime.UtcNow.AddMinutes(-2))
            {

                var tenantId = _serviceBusCredentials.TenantId;
                var clientId = _serviceBusCredentials.ClientId;
                var clientSecret = _serviceBusCredentials.ClientSecret;

                var context = new AuthenticationContext($"{authenticationLink}{tenantId}");

                AuthenticationResult result = null;

                try
                {
                    result = await context.AcquireTokenAsync(
                        azureManagementLink,
                        new ClientCredential(clientId, clientSecret)
                    );
                }
                catch (Exception e)
                {
                    var a = e;
                }


                // If the token isn't a valid string, throw an error.
                if (string.IsNullOrEmpty(result.AccessToken))
                {
                    throw new Exception("Token result is empty!");
                }

                tokenExpiresAtUtc = result.ExpiresOn.UtcDateTime;
                _token = result.AccessToken;
            }

            return _token;

        }
    }
}
