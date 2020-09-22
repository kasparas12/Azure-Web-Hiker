using System;
using System.Threading.Tasks;

using Microsoft.Azure.Management.ServiceBus;
using Microsoft.Azure.Management.ServiceBus.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;

namespace Azure.Web.Hiker.Infrastructure.ServiceBusClient
{
    public abstract class QueueCreatorBase
    {
        protected readonly IServiceBusSettings _settings;

        private const string authenticationLink = "https://login.microsoftonline.com/";
        private const string azureManagementLink = "https://management.core.windows.net/";

        private DateTime? tokenExpiresAtUtc;
        private string _token;

        public QueueCreatorBase(IServiceBusSettings settings)
        {
            _settings = settings;
        }

        public async Task CreateNewProcessingQueueForAgent(string agentHostName)
        {
            string namespaceName = GetNamespaceName();

            if (string.IsNullOrEmpty(namespaceName))
            {
                throw new Exception("Namespace name is empty!");
            }

            var token = await ObtainToken();

            var creds = new TokenCredentials(token);
            var sbClient = new ServiceBusManagementClient(creds)
            {
                SubscriptionId = _settings.SubscriptionId,
            };

            var queueParams = new SBQueue
            {
                EnablePartitioning = true
            };

            try
            {
                await sbClient.Queues.CreateOrUpdateAsync(_settings.ResourceGroupName, namespaceName, agentHostName, queueParams);
            }
            catch (Exception e)
            {
                var a = e;
            }
        }

        protected abstract string GetNamespaceName();

        private async Task<string> ObtainToken()
        {
            // Check to see if the token has expired before requesting one.
            // We will go ahead and request a new one if we are within 2 minutes of the token expiring.
            if (!tokenExpiresAtUtc.HasValue || tokenExpiresAtUtc < DateTime.UtcNow.AddMinutes(-2))
            {

                var tenantId = _settings.TenantId;
                var clientId = _settings.ClientId;
                var clientSecret = _settings.ClientSecret;

                var context = new AuthenticationContext($"{authenticationLink}{tenantId}");

                AuthenticationResult result = null;

                result = await context.AcquireTokenAsync(
                    azureManagementLink,
                    new ClientCredential(clientId, clientSecret)
                );


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

        public async Task DeleteProcessingQueueForAgent(string agentHostName)
        {
            string namespaceName = GetNamespaceName();

            if (string.IsNullOrEmpty(namespaceName))
            {
                throw new Exception("Namespace name is empty!");
            }

            var token = await ObtainToken();

            var creds = new TokenCredentials(token);
            var sbClient = new ServiceBusManagementClient(creds)
            {
                SubscriptionId = _settings.SubscriptionId,
            };

            await sbClient.Queues.DeleteAsync(_settings.ResourceGroupName, namespaceName, agentHostName);
        }
    }
}
