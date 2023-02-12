using Core.Entity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Extension;
using Shared.Network;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Core.Service
{
    public class GoogleAccessToken : IAccessToken
    {
        public string Access_token { get; set; }
        public int Expires_in { get; set; }
        public string Scope { get; set; }
        public string Token_type { get; set; }
        public string Id_token { get; set; }

        [JsonIgnore]
        public string AccessToken => Access_token;
    }

    public class GoogleUserInfo : IUserInfo
    {
        public string Sub { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string Email { get; set; }
        public bool Email_verified { get; set; }

        [JsonIgnore]
        public string Id => Sub;

        [JsonIgnore]
        public string UserName => Name;
    }

    public class GoogleService : ILoginService
    {
        private GoogleConfig _config;
        private readonly ILogger _logger;
        private readonly IHttpService _httpService;
        private readonly IUserAccountDataService _userAccountDataService;
        private readonly IDefinitionDataService _definitionService;

        public GoogleService(
            ILogger<GoogleService> logger,
            IHttpService httpService,
            IUserAccountDataService userAccountDataService,
            IDefinitionDataService definitionService)
        {
            _logger = logger;
            _httpService = httpService;
            _userAccountDataService = userAccountDataService;
            _definitionService = definitionService;
            LoadGoogleConfig();
        }

        private void LoadGoogleConfig()
        {
            _config = _definitionService.GetGeneralConfig().GoogleConfig;
        }

        public Task<string> GetLoginData(AuthType authType, string redirectUri = null)
        {
            var payload = new NameValueCollection()
            {
                ["client_id"] = _config.ClientId,
                ["response_type"] = "code",
                ["scope"] = _config.Scopes.Join(" ")
            };
            string loginUri = $"{_config.LoginUri}{_httpService.ToQueryString(payload)}";
            return Task.FromResult(loginUri);
        }

        public async Task<(TUserAccount, string)> GetUserAccount(string code, string redirectUri)
        {
            try
            {
                IAccessToken accessToken = await GetAccessToken(code, redirectUri);
                if (accessToken == null)
                    return (null, "");

                IUserInfo userInfo = await GetUserInfo(accessToken.AccessToken);
                TUserAccount account = await _userAccountDataService.GetUserAccountOrCreate(AccountType.GOOGLE, userInfo);
                return (account, "");
            }
            catch (Exception e)
            {
                _logger.LogError($"GetUserAccount failed: {e.Message}");
                return (null, "");
            }
        }

        private async Task<IUserInfo> GetUserInfo(string accessToken)
        {
            try
            {
                var payload = new NameValueCollection()
                {
                    ["access_token"] = accessToken
                };
                string userInfoUri = $"{_config.UserInfoUri}{_httpService.ToQueryString(payload)}";
                GoogleUserInfo userInfo = await _httpService.HttpGet<GoogleUserInfo>(userInfoUri);
                return userInfo;
            }
            catch (Exception e)
            {
                _logger.LogError($"GetUserInfo failed: {e.Message}");
                throw;
            }
        }

        private async Task<IAccessToken> GetAccessToken(string code, string redirectUri)
        {
            try
            {
                var payload = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("client_id", _config.ClientId),
                    new KeyValuePair<string, string>("client_secret", _config.ClientSecret),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("grant_type", "authorization_code")
                });
                GoogleAccessToken accessToken = await _httpService.HttpPost<GoogleAccessToken>(_config.TokenUri, payload);
                return accessToken;
            }
            catch (Exception e)
            {
                _logger.LogError($"GetAccessToken failed: {e.Message}");
                throw;
            }
        }
    }
}