using System.Linq;
using System.Net;

using Azure.Web.Hiker.Core.DnsResolver.Interfaces;

using Ubiety.Dns.Core;
using Ubiety.Dns.Core.Common;
using Ubiety.Dns.Core.Records.General;

namespace Azure.Web.Hiker.DNSResolver.UbietyResolver
{
    public class UbietyDnsResolver : IDnsResolver
    {
        private readonly Resolver _resolver;
        private readonly IDnsConfigureSettings _dnsConfigureSettings;
        public UbietyDnsResolver(IDnsConfigureSettings dnsConfigureSettings)
        {
            _dnsConfigureSettings = dnsConfigureSettings;

            _resolver = ResolverBuilder.Begin()
                .AddDnsServer(_dnsConfigureSettings.DnsServerIpAddress)
                .SetTimeout(_dnsConfigureSettings.DnsRequestTimeoutValue)
                .EnableCache()
                .SetRetries(_dnsConfigureSettings.DnsRequestsRetryCount)
                .UseRecursion()
                .Build();
        }

        public IPAddress ResolveHostIpAddress(string hostName)
        {
            const QuestionType questionType = QuestionType.A;

            var response = _resolver.Query(hostName, questionType);

            return response.GetRecords<RecordA>()
                .Select(record => IPAddress.Parse(record.ToString()))
                .ToHashSet()
                .FirstOrDefault();
        }
    }
}
