using Core.Entity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Network;
using System;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Core.Service
{
    public class PasswordUser : IUserInfo
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class PasswordService : ILoginService
    {
        private readonly ILogger _logger;
        private readonly IUserAccountDataService _userAccountDataService;

        public PasswordService(
            ILogger<GoogleService> logger,
            IUserAccountDataService userAccountDataService)
        {
            _logger = logger;
            _userAccountDataService = userAccountDataService;
        }

        public Task<string> GetLoginData(AuthType authType, string metaData = null)
        {
            throw new NotImplementedException();
        }

        public async Task<(TUserAccount, string)> GetUserAccount(string username, string password)
        {
            try
            {
                TUserAccount account = await _userAccountDataService.GetUserAccount(AccountType.PASSWORD, username);
                if (account == null) return (null, "Username does not exist!");
                PasswordUser metaData = JsonConvert.DeserializeObject<PasswordUser>(account.MetaData);
                if (metaData.Password != password) return (null, "The Username or Password is Incorrect!");
                return (account, "");
            }
            catch (Exception e)
            {
                _logger.LogError($"GetUserAccount failed: {e.Message}");
                return (null, "");
            }
        }
    }
}