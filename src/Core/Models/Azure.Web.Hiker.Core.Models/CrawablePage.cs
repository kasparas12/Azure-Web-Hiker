using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Azure.Web.Hiker.Core.Models
{
    public class CrawablePage
    {
        public CrawablePage(string urlString)
        {
            PageLink = new Uri(urlString);
        }

        public Uri PageLink { get; }

        public readonly List<IPAddress> ListOfHostIpv4Addresses = new List<IPAddress>();

        public void AddListOfv4Addresses(IEnumerable<string> ipAddresses)
        {
            ListOfHostIpv4Addresses.AddRange(ipAddresses.Select(x => IPAddress.Parse(x)));
        }
    }
}
