using Ardalis.GuardClauses;
using Core.Entity;
using Core.Specification;
using Core.Utility;
using System;
using System.Threading.Tasks;

namespace Core.Service
{
    public class UserDataService : IUserDataService
    {
        private readonly IRepository<TUser> _userRepository;
        private readonly IRedisDataService _redisDataService;

        public UserDataService(
            IRepository<TUser> userRepository,
            IRedisDataService redisDataService)
        {
            _userRepository = userRepository;
            _redisDataService = redisDataService;
        }

        #region Get user data

        public async Task<TUser> GetOrCreateUser(string typeAccountId)
        {
            TUser user = await GetUserByTypeAccountId(typeAccountId);
            if (user == null)
                user = await CreateNewUser();

            await RemoveCache(user.Id);
            return user;
        }

        public async Task<TUser> GetUserByIdFromCache(int userId)
        {
            TUser user = await _redisDataService.GetCacheAsync<TUser>($"user/{userId}");
            if (user == null)
                user = await GetUserById(userId);

            if (user != null)
                await UpdateCache(user);

            return user;
        }

        public async Task<TUser> GetUserById(int userId)
        {
            TUser user = await _userRepository.FirstOrDefaultAsync(new UserSpecification(userId));
            return user;
        }

        public async Task<TUser> GetUserByTypeAccountId(string typeAccountId)
        {
            TUser user = await _userRepository.FirstOrDefaultAsync(new UserSpecification(typeAccountId));
            return user;
        }

        #endregion Get user data

        #region Update user data

        public async Task<string> UpdateUserName(int userId, string userName)
        {
            TUser user = await GetUserByIdFromCache(userId);
            user.Name = userName;
            await UpdateUserAndCacheAsync(user);
            return string.Empty;
        }

        #endregion Update user data

        #region Database and cache

        private async Task<TUser> CreateNewUser()
        {
            TUser user = await _userRepository.AddAsync(new TUser());
            return user;
        }

        private async Task UpdateUser(TUser user)
        {
            await _userRepository.UpdateAsync(user);
        }

        private async Task UpdateUserAndCacheAsync(TUser user)
        {
            await UpdateUser(user);
            await UpdateCache(user);
        }

        private async Task UpdateCache(TUser user)
        {
            Guard.Against.NullUser(user);
            await _redisDataService.UpdateCacheAsync($"user/{user.Id}", user);
        }

        private async Task RemoveCache(int userId)
        {
            await _redisDataService.RemoveCacheAsync($"user/{userId}");
        }

        #endregion Database and cache

        #region Default Data for new User

        public Task AddTesterData(TUser user)
        {
            throw new NotImplementedException();
        }

        #endregion Default Data for new User
    }
}