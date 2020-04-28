using System.Collections.Generic;

namespace Azure.Web.Hiker.Core.DnsResolver.Interfaces
{
    public interface IDnsResolver<TParameter, TValue>
        where TParameter : IResolvableHost
        where TValue : IResolvedAddress
    {
        ICollection<TValue> ResolveHostIpAddress(TParameter hostName);
        ICollection<TValue> ResolveHostsIpAddresses(ICollection<TParameter> hostName);
    }
}
