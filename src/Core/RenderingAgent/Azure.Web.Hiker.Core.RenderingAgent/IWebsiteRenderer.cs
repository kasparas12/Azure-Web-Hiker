using System;
using System.Threading.Tasks;

namespace Azure.Web.Hiker.Core.RenderingAgent
{
    public interface IWebsiteRenderer
    {
        public Task<RenderResult> RenderPageAsync(Uri webPage, int timeout);
    }
}
