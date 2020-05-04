using System.Net;

namespace Azure.Web.Hiker.Core.DnsResolver.Interfaces
{
    public interface IDnsResolver
    {
        IPAddress ResolveHostIpAddress(string hostName);
    }
}
