using Ardalis.GuardClauses;
using Core.Entity;
using Core.Service;
using Core.Utility;
using MagicOnion;
using MagicOnion.Server;
using Shared.Network;
using System.Security.Claims;
using System.Text;

namespace RpcService.Service
{
    public class UserService : ServiceBase<IRpcUserService>, IRpcUserService
    {
        private readonly IUserDataService _userDataService;
        private readonly IJwtTokenValidator _jwtTokenValidator;

        public UserService(
            IUserDataService userDataService)
        {
            _userDataService = userDataService;
        }

        public async UnaryResult<UserData> SyncUserData()
        {
            TUser user = await GetUserIdentity();
            Guard.Against.NullUser(user);
            UserData userData = new()
            {
                UserId = user.EId,
                UserName = user.UserName,
            };
            return userData;
        }

        public async Task<TUser> GetUserIdentity()
        {
            var header = Context.CallContext.RequestHeaders;
            var bytes = header.GetValueBytes("auth-token-bin");
            var token = Encoding.ASCII.GetString(bytes);
            var cp = _jwtTokenValidator.GetPrincipalFromToken(token);

            if (cp != null)
            {
                var id = cp.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
                return await _userDataService.Find(id.Value);
            }
            return null;
        }
    }
}