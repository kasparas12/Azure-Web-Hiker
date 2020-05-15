using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.RenderingAgent;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using PuppeteerSharp;

namespace Azure.Web.Hiker.Infrastructure.PuppeteerRenderer
{
    public class PuppeteerRenderer : IWebsiteRenderer
    {
        private readonly string _chromiumPath;
        private readonly ILoggerFactory _loggerFactory;
        public PuppeteerRenderer(string chromiumPath, ILoggerFactory loggerFactory)
        {
            _chromiumPath = chromiumPath;
            _loggerFactory = loggerFactory;
        }

        public async Task<RenderResult> RenderPageAsync(Uri webPage, int timeout)
        {
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = _chromiumPath // @$"{context.CodePackageActivationContext.GetDataPackageObject("Data").Path}\.local-chromium\Win64-706915\chrome-win\chrome.exe"
            }, _loggerFactory).ConfigureAwait(false);

            try
            {
                using (var page = (await browser.NewPageAsync().ConfigureAwait(false)))
                {
                    Response renderResponse;

                    try
                    {
                        renderResponse = await page.GoToAsync(webPage.AbsoluteUri, timeout).ConfigureAwait(false);

                        if (renderResponse.Status != System.Net.HttpStatusCode.OK)
                        {
                            return new RenderResult(RenderStatus.OtherFailure);
                        }

                    }
                    catch (TimeoutException)
                    {
                        return new RenderResult(RenderStatus.Timeouted);
                    }

                    var parsedHrefs = await page.EvaluateFunctionAsync<dynamic>("() => { return Array.from(document.getElementsByTagName('a'), a => a.href);}").ConfigureAwait(false);

                    string jsonText = JsonConvert.SerializeObject(parsedHrefs);
                    var resultHrefs = JsonConvert.DeserializeObject<IEnumerable<string>>(jsonText);

                    return new RenderResult(RenderStatus.Ok, resultHrefs.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => new Uri(x)));
                }
            }
            catch (Exception e)
            {
                var b = e;
                return null;
            }

        }
    }
}
