using System.Threading.Tasks;

namespace Core.Service
{
    public interface IRedisDataService
    {
        Task<T> GetCacheAsync<T>(string cacheKey);

        Task UpdateCacheAsync<T>(string cacheKey, T data, float ttl = 1f, bool slidingCache = true);

        Task<bool> RemoveCacheAsync(string cacheKey);
    }
}