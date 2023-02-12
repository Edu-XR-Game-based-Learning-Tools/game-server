using Core.Entity;
using System.Threading.Tasks;

namespace Core.Service
{
    public interface IMetaDataService
    {
        Task<TMeta> CreateMeta(string metaKey, string defaultValue);

        Task<string> FindOrCreateMeta(string metaKey, string defaultValue);

        Task UpdateMetaAsync(TMeta meta);

        Task UpdateMetaCacheAsync(TMeta meta);

        Task UpdateMetaAndCacheAsync(string metaKey, string metaValue);

        Task UpdateMetaAndCacheAsync<T>(string metaKey, T metaValue);

        Task UpdateMetaAndCacheAsync(TMeta meta, string metaValue);

        Task<TMeta> GetMetaAsync(string metaKey);

        Task<string> GetMetaValueAsync(string metaKey);
    }
}