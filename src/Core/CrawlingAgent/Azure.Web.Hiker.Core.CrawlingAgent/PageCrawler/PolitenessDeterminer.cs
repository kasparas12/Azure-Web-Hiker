using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;
using Azure.Web.Hiker.Core.Common.Settings;

using Newtonsoft.Json;

using TurnerSoftware.RobotsExclusionTools;

namespace Azure.Web.Hiker.Core.CrawlingAgent.PageCrawler
{
    public interface IPolitenessDeterminer
    {
        /// <summary>
        /// Calculates the delay between consecutive webhost page crawls
        /// Negative value means do not crawl website at all
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        Task<double> CalculateHostCrawlDelayAsync(Uri webPage);
    }

    public interface IRobotsParser
    {
        Task<bool> IsUserAgentAllowedAsync(Uri webPage, string userAgent);
        Task<int?> GetCrawlDelayAsync(Uri webPage, string userAgent);
    }

    public enum DomainImportance
    {
        Undefined = -1,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10
    }

    public interface IDomainImportanceCalculator
    {
        Task<DomainImportance> CalculateDomainImportanceAsync(Uri webPage);
    }

    public class OpenRankDomainImportanceCalculator : IDomainImportanceCalculator
    {
        private readonly string _apiLink;
        private readonly string _key;

        private class OpenRankApiResponse
        {
            [JsonProperty("openrank")]
            public string OpenRank { get; set; }
        }

        public OpenRankDomainImportanceCalculator(string apiLink, string key)
        {
            _apiLink = apiLink;
            _key = key;
        }

        public async Task<DomainImportance> CalculateDomainImportanceAsync(Uri webPage)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{_apiLink}/?key={_key}&d={webPage.Host.Replace("www.", "")}");

                if (response.IsSuccessStatusCode)
                {
                    var responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());

                    if (responseObject["status"].ToString() != "ok")
                    {
                        return DomainImportance.Undefined;
                    }

                    var dataString = responseObject["data"].ToString();

                    var openRankMetric = JsonConvert.DeserializeObject<Dictionary<string, OpenRankApiResponse>>(dataString).FirstOrDefault().Value.OpenRank;

                    var parsed = int.Parse(openRankMetric);

                    if (parsed != -1)
                    {
                        parsed /= 10;
                    }

                    return (DomainImportance)parsed;
                }

                return DomainImportance.Undefined;
            }
        }
    }

    public class TurnerRobotsParser : IRobotsParser
    {
        private readonly RobotsFileParser _robotsFileParser;
        private RobotsFile _robotsFile;
        public TurnerRobotsParser()
        {
            _robotsFileParser = new RobotsFileParser();
        }

        public async Task<int?> GetCrawlDelayAsync(Uri webPage, string userAgent)
        {
            var robotsFile = await GetRobotsFileAsync(webPage);

            if (robotsFile == null)
            {
                return null;
            }

            var entryForUserAgent = robotsFile.GetEntryForUserAgent(userAgent);

            return entryForUserAgent != null ? entryForUserAgent.CrawlDelay : null;
        }

        public async Task<bool> IsUserAgentAllowedAsync(Uri webPage, string userAgent)
        {
            var robotsFile = await GetRobotsFileAsync(webPage);

            if (robotsFile == null)
            {
                return true;
            }

            var isAllowed = robotsFile.IsAllowedAccess(webPage, userAgent);

            return isAllowed;
        }

        private async Task<RobotsFile> GetRobotsFileAsync(Uri webPage)
        {
            if (_robotsFile != null)
            {
                return _robotsFile;
            }

            var robotsUrl = new Uri(webPage.Scheme + "://" + webPage.Host + "/robots.txt");
            var robotsFile = await _robotsFileParser.FromUriAsync(robotsUrl);
            _robotsFile = robotsFile;

            return robotsFile;
        }
    }

    public class DefaultPolitenessDeterminer : IPolitenessDeterminer
    {
        private readonly IGeneralApplicationSettings _generalApplicationSettings;
        private readonly IRobotsParser _robotsFileParser;
        private readonly IDomainImportanceCalculator _domainImportanceCalculator;
        private readonly IAgentRegistrarRepository _repository;

        public DefaultPolitenessDeterminer(IGeneralApplicationSettings generalApplicationSettings, IRobotsParser robotsFileParser, IDomainImportanceCalculator domainImportanceCalculator, IAgentRegistrarRepository agentRegistrarRepository)
        {
            _generalApplicationSettings = generalApplicationSettings;
            _robotsFileParser = robotsFileParser;
            _domainImportanceCalculator = domainImportanceCalculator;
            _repository = agentRegistrarRepository;
        }
        public async Task<double> CalculateHostCrawlDelayAsync(Uri webPage)
        {
            var precalculatedDelay = _repository.GetPrecalculatedCrawlDelay(webPage.Host);

            if (precalculatedDelay != null)
            {
                return precalculatedDelay.Value;
            }

            var isUserAgentAllowed = await _robotsFileParser.IsUserAgentAllowedAsync(webPage, _generalApplicationSettings.CrawlerUserAgent);

            if (!isUserAgentAllowed)
            {
                return -1;
            }

            var serverResponseTime = await CalculateServerResponseTimeAsync(webPage);

            if (serverResponseTime == -1)
            {
                return -1;
            }

            var robotsCrawlDelay = await _robotsFileParser.GetCrawlDelayAsync(webPage, _generalApplicationSettings.CrawlerUserAgent);
            var robotsCrawlDelayTime = robotsCrawlDelay.HasValue ? robotsCrawlDelay.Value : 0;

            var domainImportanceTime = await CalculateDomainImportanceTimeAsync(webPage);

            double responseTime = Math.Pow(0.203137637588 + 0.724386103344 * serverResponseTime, 2);

            double result = Math.Max(domainImportanceTime, responseTime);

            result = Math.Max(result, robotsCrawlDelayTime);

            result = Math.Max(result, _generalApplicationSettings.MinCrawlDelaySeconds);

            result = Math.Min(result, _generalApplicationSettings.MaxCrawlDelaySeconds);

            _repository.InsertCalculatedCrawlDelay(webPage.Host, result);

            return result;
        }

        private async Task<double> CalculateServerResponseTimeAsync(Uri webPage)
        {
            using (var client = new HttpClient())
            {
                var startTime = DateTime.UtcNow;
                var response = await client.GetAsync(webPage);
                var endTime = DateTime.UtcNow;

                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    return -1;
                }

                return (endTime - startTime).TotalMilliseconds / (double)1000;
            }
        }

        private async Task<double> CalculateDomainImportanceTimeAsync(Uri webPage)
        {
            var importance = await _domainImportanceCalculator.CalculateDomainImportanceAsync(webPage);

            int importanceInt;

            if (importance == DomainImportance.Undefined)
            {
                importanceInt = 0;
            }
            else
            {
                importanceInt = (int)importance;
            }
            double workSizeTime = Math.Min(Math.Exp(2.52166863221 + -0.530185027289 * Math.Log(importanceInt)), 5);

            return workSizeTime;
        }
    }
}

