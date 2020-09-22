namespace Azure.Web.Hiker.Core.DnsResolver.Interfaces
{
    public interface IDnsConfigureSettings
    {
        public string DnsServerIpAddress { get; }

        public int DnsRequestsRetryCount { get; }

        public int DnsRequestTimeoutValue { get; }
    }
}
