using Core.Entity;
using System.Threading.Tasks;

namespace Core.Service
{
    public interface IUserAccountDataService
    {
        Task<TUserAccount> GetUserAccountOrCreate(AccountType type, IUserInfo userInfo);

        Task<TUserAccount> GetAccount(AccountType type, string accountId);

        Task<TUserAccount> GetUserAccount(AccountType type, string accountId);

        Task<TUserAccount> GetAccountByMetaData(AccountType type, string accountId);

        Task<TUserAccount> CreateAccount(AccountType type, IUserInfo userInfo);

        Task<string> GetUserSessionCache(int userId);

        Task UpdateSessionCache(int userId, string sessionId);
    }
}