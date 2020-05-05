namespace Azure.Web.Hiker.Infrastructure.ServiceBusClient
{
    public interface IServiceBusSettings
    {
        public string ServiceBusConnectionString { get; set; }
        public string CrawlingFrontQueueName { get; set; }
        public string CreateAgentQueue { get; set; }
    }
}
