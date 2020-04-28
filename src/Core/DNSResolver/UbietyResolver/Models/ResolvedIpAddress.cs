using System.Net;

using Azure.Web.Hiker.Core.DnsResolver.Interfaces;

namespace Azure.Web.Hiker.DNSResolver.UbietyResolver.Models
{
    public class ResolvedIpAddress : IResolvedAddress
    {
        public IPAddress Ipv4Address { get; set; }
    }
}
