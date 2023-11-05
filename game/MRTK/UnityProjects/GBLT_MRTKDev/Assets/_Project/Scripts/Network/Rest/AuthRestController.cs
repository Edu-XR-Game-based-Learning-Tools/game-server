using Core.Business;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Proyecto26;
using Shared.Network;
using System;

namespace Core.Network
{
    public class AuthRestController : IRpcAuthController, IDisposable
    {
        private readonly EndPointSwitcher _endPointSwitcher;
        private readonly UserAuthentication _userAuthentication;

        public bool IsExpired => _userAuthentication == null || _userAuthentication.IsExpired;

        public AuthRestController(
            EndPointSwitcher endPointSwitcher,
            UserAuthentication userAuthenticationData)
        {
            _endPointSwitcher = endPointSwitcher;
            _userAuthentication = userAuthenticationData;
        }

        public void Dispose()
        { }

        public void ClearAuthenticationData()
        {
            _userAuthentication.ClearAuthenticationData();
        }

        public async UniTask<AuthenticationData> Login(LoginRequest request)
        {
            bool isDoneRequest = false;
            AuthenticationData data = null;
            RestClient.Post($"{_endPointSwitcher.ApiEndPoint}/api/auth/login", JsonConvert.SerializeObject(request)).Then(response =>
            {
                data = JsonConvert.DeserializeObject<AuthenticationData>(response.Text);
                isDoneRequest = true;
            });
            await UniTask.WaitUntil(() => isDoneRequest);

            return data;
        }

        public async UniTask<AuthenticationData> Register(RegisterRequest request)
        {
            bool isDoneRequest = false;
            AuthenticationData data = null;
            RestClient.Post($"{_endPointSwitcher.ApiEndPoint}/api/auth/register", request).Then(response =>
            {
                data = JsonConvert.DeserializeObject<AuthenticationData>(response.Text);
                isDoneRequest = true;
            });
            await UniTask.WaitUntil(() => isDoneRequest);

            return data;
        }

        public async UniTask<AuthenticationData> RefreshToken(ExchangeRefreshTokenRequest request)
        {
            bool isDoneRequest = false;
            AuthenticationData data = null;
            var requestHelper = new RequestHelper
            {
                Method = "POST",
                Uri = $"{_endPointSwitcher.ApiEndPoint}/api/auth/refreshToken",
                Body = request,
            };
            RestClient.Request(requestHelper).Then(response =>
            {
                data = JsonConvert.DeserializeObject<AuthenticationData>(response.Text);
                isDoneRequest = true;
            });
            await UniTask.WaitUntil(() => isDoneRequest);

            return data;
        }
    }
}
