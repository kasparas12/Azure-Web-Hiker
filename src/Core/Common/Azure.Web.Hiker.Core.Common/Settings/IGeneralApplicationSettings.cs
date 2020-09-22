namespace Azure.Web.Hiker.Core.Common.Settings
{
    public interface IGeneralApplicationSettings
    {
        public int MaxNumberOfAgents { get; set; }
        public int AgentInactivityTimeoutValue { get; set; }
        public string CrawlerUserAgent { get; set; }
        public double MinCrawlDelaySeconds { get; set; }
        public double MaxCrawlDelaySeconds { get; set; }
    }
}
