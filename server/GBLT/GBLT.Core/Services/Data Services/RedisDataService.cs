using Core.Utility;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using RedLockNet;

namespace Core.Service
{
    public class RedisDataService : IRedisDataService
    {
        private readonly IDistributedCache _cache;
        private readonly IDistributedLockFactory _redisLockFactory;

        private readonly string _prefix;

        public RedisDataService(
            IDistributedCache cache,
            IDistributedLockFactory redisLockFactory,
            IConfiguration configuration)
        {
            _cache = cache;
            _redisLockFactory = redisLockFactory;
            _prefix = configuration.GetValue<string>("Environment") + "/";
        }

        public async Task UpdateCacheAsync<T>(string cacheKey, T data, float ttl = 1f, bool slidingCache = true)
        {
            DistributedCacheEntryOptions options;
            if (slidingCache)
                options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(ttl));
            else
                options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(ttl));

            await CacheLockAndUpdateAsync(cacheKey.ToLower(), data, options);
        }

        public async Task<T> GetCacheAsync<T>(string cacheKey)
        {
            byte[] dataByte = await _cache.GetAsync(_prefix + cacheKey.ToLower());
            if (dataByte != null)
                return dataByte.Deserialize<T>();

            return default;
        }

        public async Task<bool> RemoveCacheAsync(string cacheKey)
        {
            await _cache.RemoveAsync(_prefix + cacheKey.ToLower());
            return true;
        }

        private async Task CacheLockAndUpdateAsync<T>(string cacheKey, T data, DistributedCacheEntryOptions options)
        {
            try
            {
                using var redLock = await _redisLockFactory.CreateLockAsync(
                    cacheKey,
                    expiryTime: TimeSpan.FromSeconds(30),
                    waitTime: TimeSpan.FromSeconds(1),
                    retryTime: TimeSpan.FromSeconds(0.1));

                if (redLock.IsAcquired)
                    await _cache.SetAsync(_prefix + cacheKey, data.Serialize(), options);
            }
            catch
            {
                throw;
            }
        }
    }
}