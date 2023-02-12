using Core.Entity;
using Core.Specification;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Core.Service
{
    public class UserAccountDataService : IUserAccountDataService
    {
        private readonly string _userSessionPrefix = "user-session/user_";

        private readonly IUserDataService _userDataService;
        private readonly IRedisDataService _redisDataService;
        private readonly IRepository<TUserAccount> _userAccountRepository;

        public UserAccountDataService(
            IUserDataService userDataService,
            IRedisDataService redisDataService,
            IRepository<TUserAccount> userAccountRepository)
        {
            _userDataService = userDataService;
            _redisDataService = redisDataService;
            _userAccountRepository = userAccountRepository;
        }

        public async Task<TUserAccount> GetUserAccountOrCreate(AccountType type, IUserInfo userInfo)
        {
            TUserAccount userAccount = await GetUserAccount(type, accountId: userInfo.Id);
            if (userAccount == null)
                userAccount = await CreateAccount(type, userInfo);

            return userAccount;
        }

        public async Task<TUserAccount> GetAccount(AccountType type, string accountId)
        {
            TUserAccount account = await _userAccountRepository.FirstOrDefaultAsync(new AccountSpecification(type, accountId));
            return account;
        }

        public async Task<TUserAccount> GetUserAccount(AccountType type, string accountId)
        {
            TUserAccount userAccount = await _userAccountRepository.FirstOrDefaultAsync(new UserAccountSpecification(type, accountId));
            return userAccount;
        }

        public async Task<TUserAccount> GetAccountByMetaData(AccountType type, string walletAddress)
        {
            string metaData = FormatTypeAccountId(type, walletAddress);
            TUserAccount account = await _userAccountRepository.FirstOrDefaultAsync(new AccountByMetaDataSpecification(metaData));
            return account;
        }

        public async Task<TUserAccount> CreateAccount(AccountType type, IUserInfo userInfo)
        {
            string accountId = userInfo.Id.ToLower();
            string metaData = JsonConvert.SerializeObject(userInfo);
            string typeAccountId = FormatTypeAccountId(type, accountId);

            TUser user = await _userDataService.GetOrCreateUser(typeAccountId);
            TUserAccount account = await _userAccountRepository.AddAsync(new TUserAccount(type, accountId, user.Id, metaData));
            account.User = user;
            return account;
        }

        private async Task UpdateAccount(TUserAccount account)
        {
            await _userAccountRepository.UpdateAsync(account);
        }

        public async Task<string> GetUserSessionCache(int userId)
        {
            return await _redisDataService.GetCacheAsync<string>($"{_userSessionPrefix}{userId}");
        }

        public async Task UpdateSessionCache(int userId, string sessionId)
        {
            await _redisDataService.UpdateCacheAsync($"{_userSessionPrefix}{userId}", sessionId);
        }

        private static string FormatTypeAccountId(AccountType type, string accountId)
        {
            return string.Format("{0}_{1}", type, accountId).ToLower();
        }
    }
}