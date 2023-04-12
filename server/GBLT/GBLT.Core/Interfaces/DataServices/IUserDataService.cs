using Core.Dto;
using Core.Entity;
using Shared.Network;

namespace Core.Service
{
    public interface IUserDataService
    {
        Task<GeneralResponse> Create(RegisterRequest message);

        Task<TUser> Find(string identityId);

        Task<TUser> FindByName(string userName);

        Task<bool> CheckPassword(TUser user, string password);

        Task<string[]> GetUserRoles(string identityId);

        Task Update(TUser user);
    }
}