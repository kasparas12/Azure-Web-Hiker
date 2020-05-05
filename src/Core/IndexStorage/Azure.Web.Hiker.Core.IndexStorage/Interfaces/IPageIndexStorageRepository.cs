using System.Threading.Tasks;

using Azure.Web.Hiker.Core.IndexStorage.Models;

namespace Azure.Web.Hiker.Core.IndexStorage.Interfaces
{
    public interface IPageIndexStorageRepository<T> where T : class, IPageIndex, new()
    {
        Task InsertNewPageIndex(T entity);
        Task<T> GetPageIndexByUrl(string url);
        Task<bool> IsPageIndexAlreadyVisited(string url);
    }
}
