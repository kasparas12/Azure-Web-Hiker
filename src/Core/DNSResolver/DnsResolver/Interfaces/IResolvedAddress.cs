using System.Net;

namespace Azure.Web.Hiker.Core.DnsResolver.Interfaces
{
    public interface IResolvedAddress
    {
        public IPAddress Ipv4Address { get; set; }
    }
}
