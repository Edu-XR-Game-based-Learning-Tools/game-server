using Ardalis.GuardClauses;
using Core.Entity;
using Core.Specification;
using System.Threading.Tasks;

namespace Core.Service
{
    public class MetaDataService : IMetaDataService
    {
        private readonly string _metaRedisPrefix = "meta/";

        private readonly IRepository<TMeta> _metaRepository;
        private readonly IRedisDataService _redisDataService;

        public MetaDataService(
            IRepository<TMeta> metaRepository,
            IRedisDataService redisDataService)
        {
            _metaRepository = metaRepository;
            _redisDataService = redisDataService;
        }

        public async Task<TMeta> CreateMeta(string metaKey, string defaultValue)
        {
            TMeta meta = await GetOrCreateMeta(metaKey, defaultValue);
            return meta;
        }

        public async Task<string> FindOrCreateMeta(string metaKey, string defaultValue)
        {
            TMeta meta = await GetMetaFromCache(metaKey);
            if (meta == null)
                meta = await GetOrCreateMeta(metaKey, defaultValue);

            return meta.MetaValue;
        }

        public async Task UpdateMetaAsync(TMeta meta)
        {
            await _metaRepository.UpdateAsync(meta);
        }

        public async Task UpdateMetaCacheAsync(TMeta meta)
        {
            if (meta != null)
                await _redisDataService.UpdateCacheAsync($"{_metaRedisPrefix}{meta.MetaKey}", meta);
        }

        public async Task UpdateMetaAndCacheAsync(string metaKey, string metaValue)
        {
            TMeta meta = await GetMetaAsync(metaKey);
            meta.MetaValue = metaValue;
            await UpdateMetaAsync(meta);
            await UpdateMetaCacheAsync(meta);
        }

        public async Task UpdateMetaAndCacheAsync<T>(string metaKey, T metaValue)
        {
            TMeta meta = await GetMetaAsync(metaKey);
            meta.MetaValue = metaValue.ToString();
            await UpdateMetaAsync(meta);
            await UpdateMetaCacheAsync(meta);
        }

        public async Task UpdateMetaAndCacheAsync(TMeta meta, string metaValue)
        {
            meta.MetaValue = metaValue;
            await UpdateMetaAsync(meta);
            await UpdateMetaCacheAsync(meta);
        }

        public async Task<TMeta> GetMetaAsync(string metaKey)
        {
            Guard.Against.NullOrEmpty(metaKey, nameof(metaKey));
            var meta = await GetMetaFromCache(metaKey);
            if (meta == null)
            {
                meta = await _metaRepository.FirstOrDefaultAsync(new MetaSpecification(metaKey));
                await UpdateMetaCacheAsync(meta);
            }
            return meta;
        }

        public async Task<string> GetMetaValueAsync(string metaKey)
        {
            TMeta meta = await GetMetaAsync(metaKey);
            return meta?.MetaValue;
        }

        private async Task<TMeta> GetMetaFromCache(string metaKey)
        {
            TMeta meta = await _redisDataService.GetCacheAsync<TMeta>($"{_metaRedisPrefix}{metaKey}");
            return meta;
        }

        private async Task<TMeta> GetOrCreateMeta(string metaKey, string defaultMetaValue)
        {
            TMeta meta = await _metaRepository.FirstOrDefaultAsync(new MetaSpecification(metaKey));
            if (meta == null)
                meta = await _metaRepository.AddAsync(new TMeta(metaKey, defaultMetaValue));

            return meta;
        }
    }
}