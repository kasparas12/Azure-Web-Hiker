using Azure.Web.Hiker.Core.DnsResolver.Interfaces;

namespace Azure.Web.Hiker.DNSResolver.UbietyResolver.Models
{
    public class HostToResolve : IResolvableHost
    {
        public HostToResolve(string hostToResolve)
        {
            HostAddress = hostToResolve;
        }
        public string HostAddress { get; }
    }
}
