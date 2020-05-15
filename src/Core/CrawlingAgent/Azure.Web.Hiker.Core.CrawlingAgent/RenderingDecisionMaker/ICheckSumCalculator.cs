using System;
using System.Net;
using System.Security.Cryptography;

namespace Azure.Web.Hiker.Core.CrawlingAgent.RenderingDecisionMaker
{
    public interface ICheckSumCalculator
    {
        string CalulateChecksum(Uri webPage);
    }

    public class MD5ChecksumCalculator : ICheckSumCalculator
    {
        public string CalulateChecksum(Uri webPage)
        {
            using (var md5 = MD5.Create())

            using (var webClient = new WebClient())
            using (var stream = webClient.OpenRead(webPage))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
