using Core.Entity;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Shared.Network;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Core.Service
{
    public class FirebaseUser : IUserInfo
    {
        public string Name { get; set; }
        public string PictureUrl { get; set; }
        public DateTime AuthTime { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public DateTime AuthExpireTime { get; set; }

        public string Id => UserId;

        public string UserName => Name;

        public string Email => UserEmail;
    }

    public class FirebaseService : ILoginService
    {
        private readonly ILogger _logger;
        private readonly IUserAccountDataService _userAccountDataService;
        private readonly IDefinitionDataService _definitionDataService;
        private FirebaseConfig _firebaseConfig;

        public FirebaseService(
            ILogger<FirebaseService> logger,
            IDefinitionDataService definitionDataService,
            IUserAccountDataService userAccountDataService)
        {
            _logger = logger;
            _userAccountDataService = userAccountDataService;
            _definitionDataService = definitionDataService;
            GetConfiguration();
        }

        private void GetConfiguration()
        {
            _firebaseConfig = _definitionDataService.GetGeneralConfig().FirebaseConfig;
        }

        public async Task<string> GetLoginData(AuthType authType, string metaData = null)
        {
            return await Task.FromResult("IgnoreStep");
        }

        public async Task<(TUserAccount, string)> GetUserAccount(string idToken, string userEmail)
        {
            try
            {
                IUserInfo userInfo = await GetUserInfo(idToken);
                (userInfo as FirebaseUser).UserEmail = userEmail;
                TUserAccount account = await _userAccountDataService.GetUserAccountOrCreate(AccountType.FIREBASE, userInfo);
                return (account, "");
            }
            catch (Exception e)
            {
                _logger.LogError($"GetUserAccount failed: {e.Message}");
                return (null, "");
            }
        }

        private async Task<IUserInfo> GetUserInfo(string idToken)
        {
            IUserInfo user = await GetFirebaseUserInfo(idToken);
            return user;
        }

        private async UniTask<FirebaseUser> GetFirebaseUserInfo(string token)
        {
            string hashChunk = token;
            //token = token.Replace('-', '+').Replace('_', '/');
            //string[] sections = token.Split('.');

            var publicKeyDictionary = await GetPublicKey();
            if (publicKeyDictionary == null) return null;

            var principal = VerifyToken(publicKeyDictionary, hashChunk);
            if (principal == null) { return null; }

            if (!VerifyPayload(principal)) { return null; }
            return ExactFirebaseUserInfo(principal);
        }

        private static async UniTask<Dictionary<string, string>> GetPublicKey()
        {
            HttpClient client = new()
            {
                BaseAddress = new Uri("https://www.googleapis.com/robot/v1/metadata/")
            };

            var response = await client.GetAsync("x509/securetoken@system.gserviceaccount.com");
            if (!response.IsSuccessStatusCode) { return null; }
            string x509Data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(x509Data);
        }

        private ClaimsPrincipal VerifyToken(Dictionary<string, string> publicKeyDict, string hashChunk)
        {
            //Verify id token by public key
            SecurityKey[] keys = publicKeyDict.Values.Select(CreateSecurityKeyFromPublicKey).ToArray();
            var parameters = new TokenValidationParameters
            {
                ValidIssuer = _firebaseConfig.Issuer,
                ValidAudience = _firebaseConfig.Audience,
                IssuerSigningKeys = keys,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.ValidateToken(hashChunk, parameters, out SecurityToken validatedToken);
        }

        private bool VerifyPayload(ClaimsPrincipal principal)
        {
            var iss = principal.Claims.FirstOrDefault(x => x.Type == "iss").Value;
            var aud = principal.Claims.FirstOrDefault(x => x.Type == "aud").Value;
            return iss == _firebaseConfig.Issuer && aud == _firebaseConfig.Audience;
        }

        public FirebaseUser ExactFirebaseUserInfo(ClaimsPrincipal principal)
        {
            FirebaseUser firebaseUser = new()
            {
                UserId = principal.Claims.FirstOrDefault(x => x.Type == "user_id").Value,
                Name = principal.Claims.FirstOrDefault(x => x.Type == "name").Value,
                PictureUrl = principal.Claims.FirstOrDefault(x => x.Type == "picture").Value,
                AuthTime = ConvertToDateTime(principal.Claims.FirstOrDefault(x => x.Type == "auth_time").Value),
                AuthExpireTime = ConvertToDateTime(principal.Claims.FirstOrDefault(x => x.Type == "exp").Value)
            };

            return firebaseUser;
        }

        private static SecurityKey CreateSecurityKeyFromPublicKey(string data)
        {
            return new X509SecurityKey(new X509Certificate2(Encoding.UTF8.GetBytes(data)));
        }

        private static DateTime ConvertToDateTime(string value)
        {
            long unixTime = long.Parse(value);
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
        }
    }
}