using Core.Entity;
using System.Threading.Tasks;

namespace Core.Service
{
    public interface IUserDataService
    {
        Task<TUser> GetOrCreateUser(string typeAccountId);

        Task<TUser> GetUserById(int userId);

        Task<TUser> GetUserByIdFromCache(int userId);

        Task<TUser> GetUserByTypeAccountId(string typeAccountId);

        Task<string> UpdateUserName(int userId, string userName);

        Task AddTesterData(TUser user);
    }
}