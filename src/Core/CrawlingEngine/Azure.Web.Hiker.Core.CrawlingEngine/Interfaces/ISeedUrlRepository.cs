using System.Collections.Generic;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.CrawlingEngine.Models;

namespace Azure.Web.Hiker.Core.CrawlingEngine.Interfaces
{
    public interface ISeedUrlRepository
    {
        Task<IEnumerable<SeedUrlAddress>> GetListOfSeedUrls();
    }
}
