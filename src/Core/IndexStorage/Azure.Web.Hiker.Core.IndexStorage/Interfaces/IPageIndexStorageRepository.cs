using System.Collections.Generic;
using System.Threading.Tasks;

using Azure.Web.Hiker.Core.IndexStorage.Models;

namespace Azure.Web.Hiker.Core.IndexStorage.Interfaces
{
    public interface IPageIndexStorageRepository
    {
        Task InsertOrMergeNewPageIndex(IPageIndex entity);
        Task<IPageIndex> GetPageIndexByUrl(string url);
        Task<IEnumerable<string>> FilterUnvisitedLinks(IEnumerable<string> urls);
    }
}
