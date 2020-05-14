namespace Azure.Web.Hiker.Infrastructure.ServiceBusClient
{
    public interface IServiceBusSettings
    {
        public string ServiceBusConnectionString { get; set; }
        public string RenderingServiceBusConnectionString { get; set; }
        public string RenderingQueue { get; set; }
        public string AgentCreateQueue { get; set; }
        public string CWCEControlQueue { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CrawlingServiceBusNamespaceName { get; set; }
        public string RenderingServiceBusNamespaceName { get; set; }
        public string SubscriptionId { get; set; }
        public string ResourceGroupName { get; set; }
    }
}
