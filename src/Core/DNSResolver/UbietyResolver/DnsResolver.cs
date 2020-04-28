using System.Collections.Generic;
using System.Linq;
using System.Net;

using Azure.Web.Hiker.Core.DnsResolver.Interfaces;
using Azure.Web.Hiker.DNSResolver.UbietyResolver.Models;

using Ubiety.Dns.Core;
using Ubiety.Dns.Core.Common;
using Ubiety.Dns.Core.Records.General;

namespace Azure.Web.Hiker.DNSResolver.UbietyResolver
{
    public class UbietyDnsResolver : IDnsResolver<HostToResolve, ResolvedIpAddress>
    {
        private readonly Resolver _resolver;

        public UbietyDnsResolver()
        {
            _resolver = ResolverBuilder.Begin()
                .AddDnsServer("81.7.114.190")
                .SetTimeout(1000)
                .EnableCache()
                .SetRetries(3)
                .UseRecursion()
                .Build();
        }

        public ICollection<ResolvedIpAddress> ResolveHostIpAddress(HostToResolve hostName)
        {
            const QuestionType questionType = QuestionType.A;

            var response = _resolver.Query(hostName.HostAddress, questionType);

            return response.GetRecords<RecordA>().Select(record => new ResolvedIpAddress { Ipv4Address = IPAddress.Parse(record.ToString()) }).ToHashSet();
        }

        public ICollection<ResolvedIpAddress> ResolveHostsIpAddresses(ICollection<HostToResolve> hostName)
        {
            var first = hostName.First();

            return ResolveHostIpAddress(first);
        }
    }
}
