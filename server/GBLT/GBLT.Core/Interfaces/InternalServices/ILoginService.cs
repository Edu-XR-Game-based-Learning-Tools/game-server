using Core.Entity;
using Shared.Network;
using System.Threading.Tasks;

namespace Core.Service
{
    public interface IAccessToken
    {
        string AccessToken { get; }
    }

    public interface IUserInfo
    {
        string Id { get; }
        string UserName { get; }
        string Email { get; }
    }

    public interface ILoginService
    {
        Task<string> GetLoginData(AuthType authType, string metaData = null);

        Task<(TUserAccount, string)> GetUserAccount(string signature, string walletAddress);
    }
}