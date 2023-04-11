using Core.Entity;
using Core.Service;
using Core.Utility;
using MagicOnion;
using MagicOnion.Server;
using MessagePipe;
using Microsoft.AspNetCore.Authorization;
using RpcService.Authentication;
using RpcService.Configuration;
using Shared.Network;

namespace RpcService.Service
{
    [Authorize]
    public class AuthenService : ServiceBase<IAuthServices>, IAuthServices
    {
        private ILoginService _loginService;
        private readonly LoginServiceResolver _loginServiceAccessor;
        private readonly IUserAccountDataService _userAccountDataService;
        private readonly JwtTokenService _jwtTokenService;
        private readonly IPublisher<(int, string)> _sessionPublisher;
        private readonly IUserDataService _userDataService;

        public AuthenService(
            IPublisher<(int, string)> sessionPublisher,
            LoginServiceResolver loginServiceAccessor,
            JwtTokenService jwtTokenService,
            IUserAccountDataService userAccountDataService,
            IUserDataService userDataService)
        {
            _jwtTokenService = jwtTokenService;
            _sessionPublisher = sessionPublisher;
            _loginServiceAccessor = loginServiceAccessor;
            _userAccountDataService = userAccountDataService;
            _userDataService = userDataService;
        }

        public async UnaryResult<string> GetLoginData(AuthType authType, string metaData = "")
        {
            CheckLoginType(authType);
            string loginData = await _loginService.GetLoginData(authType, metaData);
            return loginData;
        }

        [AllowAnonymous]
        public async UnaryResult<AuthenticationData> SignIn(SignInData signInData)
        {
            CheckLoginType(signInData.AuthType);
            (TUserAccount userAccount, string msg) = await _loginService.GetUserAccount(signInData.Code, signInData.MetaData);
            if (userAccount == null)
                return string.IsNullOrEmpty(msg) ? AuthenticationData.Failed : AuthenticationData.Failed.UpdateMessage(msg);

            AuthenticationData authData = await PrepareAuthenticationData(userAccount, signInData.AuthType);
            await AddDataToUserByEnvironmentOrRole(userAccount.User);
            return authData;
        }

        private Task AddDataToUserByEnvironmentOrRole(TUser user)
        {
            if (user.Role == UserRole.Tester)
                return _userDataService.AddTesterData(user);

            return Task.CompletedTask;
        }

        private void CheckLoginType(AuthType authType)
        {
            AccountType accountType = authType.GetAccountType();
            if (accountType != AccountType.PASSWORD && accountType != AccountType.GOOGLE && accountType != AccountType.FIREBASE)
                throw new NotImplementedException($"Login type not supported {authType}");

            _loginService = _loginServiceAccessor(accountType);
        }

        private async Task<AuthenticationData> PrepareAuthenticationData(TUserAccount userAccount, AuthType authType)
        {
            string sessionId = await HandleUserSession(userAccount.User.Id);
            CustomJwtAuthUserIdentity jwt = new()
            {
                Id = userAccount.User.Id,
                UserId = userAccount.User.UserId,
                AccountId = userAccount.AccountId,
                AccountType = userAccount.Type,
                Name = userAccount.User.Name,
                SessionId = sessionId,
                AuthSource = authType,
            };
            JwtAuthenticationTokenResult encodedPayload = _jwtTokenService.CreateToken(jwt);
            AuthenticationData authData = new()
            {
                Id = userAccount.User.Id,
                UserId = userAccount.User.UserId,
                AccountId = userAccount.AccountId,
                UserName = userAccount.User.Name,
                Expiration = encodedPayload.Expiration,
                AuthToken = encodedPayload.Token,
                AuthSource = authType,
                Success = true
            };
            return authData;
        }

        private async Task<string> HandleUserSession(int userId)
        {
            string sessionId = $"user_{userId}-{Guid.NewGuid()}";
            _sessionPublisher.Publish((userId, sessionId));
            await _userAccountDataService.UpdateSessionCache(userId, sessionId);
            return sessionId;
        }
    }
}