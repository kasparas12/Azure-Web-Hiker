using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.RenderingAgent;

using Newtonsoft.Json;

using PuppeteerSharp;

namespace Azure.Web.Hiker.Infrastructure.PuppeteerRenderer
{
    public class PuppeteerRenderer : IWebsiteRenderer
    {
        public async Task<RenderResult> RenderPageAsync(Uri webPage, int timeout)
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });

            using (var page = (await browser.NewPageAsync()))
            {
                Response renderResponse;

                try
                {
                    renderResponse = await page.GoToAsync(webPage.AbsoluteUri, timeout);

                    if (renderResponse.Status != System.Net.HttpStatusCode.OK)
                    {
                        return new RenderResult(RenderStatus.OtherFailure);
                    }

                }
                catch (TimeoutException)
                {
                    return new RenderResult(RenderStatus.Timeouted);
                }

                var parsedHrefs = await page.EvaluateFunctionAsync<dynamic>("() => { return Array.from(document.getElementsByTagName('a'), a => a.href);}");

                string jsonText = JsonConvert.SerializeObject(parsedHrefs);
                var resultHrefs = JsonConvert.DeserializeObject<IEnumerable<string>>(jsonText);

                return new RenderResult(RenderStatus.Ok, resultHrefs.Select(x => new Uri(x)));
            }
        }
    }
}
