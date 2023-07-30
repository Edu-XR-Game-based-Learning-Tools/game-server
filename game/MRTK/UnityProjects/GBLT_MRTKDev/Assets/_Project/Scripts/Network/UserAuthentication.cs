using Core.EventSignal;
using Core.Framework;
using MessagePipe;
using Newtonsoft.Json;
using Shared.Network;
using System;

namespace Core.Network
{
    public class UserAuthentication
    {
        public const string AuthenTokenKey = "SEC_TOKEN";

        private readonly object _syncObject = new();

        public string Token => _authenticationData != null && _authenticationData.AccessToken != null ? _authenticationData.AccessToken.Token : null;

        public bool IsExpired
        {
            get
            {
                return _authenticationData == null || _authenticationData.IsExpired();
            }
        }

        private AuthenticationData _authenticationData;
        public AuthType AuthType => _authenticationData != null ? _authenticationData.AuthSource : AuthType.FAILED;

        public bool IsSignedIn => _authenticationData != null && !IsExpired;

        public UserAuthentication()
        {
            _authenticationData = GetAuthenticationData();
        }

        public AuthenticationData Update(AuthenticationData data)
        {
            lock (_syncObject)
            {
                if (data.AccessToken == null)
                    throw new ArgumentNullException(nameof(data.AccessToken));

                _authenticationData = TryGetOrCreateNewAuthenData();
                _authenticationData.AccessToken = data.AccessToken;
                _authenticationData.AuthSource = data.AuthSource;
                _authenticationData.UserName = data.UserName;
                _authenticationData.Success = data.Success;
                SetAuthenticationData(_authenticationData);
                return _authenticationData;
            }
        }

        private AuthenticationData TryGetOrCreateNewAuthenData()
        {
            AuthenticationData result = GetAuthenticationData();
            if (result == null)
            {
                return new AuthenticationData();
            }
            return result;
        }

        public AuthenticationData GetAuthenticationData()
        {
            if (!IsExpired)
                return _authenticationData;
            AuthenticationData result = null;
            var authenData = PlayerPrefManager.GetEncryptedString(AuthenTokenKey);
            if (!string.IsNullOrEmpty(authenData))
            {
                try
                {
                    result = JsonConvert.DeserializeObject<AuthenticationData>(authenData);
                    return result.IsExpired() ? null : result;
                }
                catch
                {
                    return null;
                }
            }
            return result;
        }

        private void SetAuthenticationData(AuthenticationData value)
        {
            _authenticationData = value;
            var json = JsonConvert.SerializeObject(value);
            PlayerPrefManager.SetEncryptedString(AuthenTokenKey, json);
        }

        public bool TryGetAuthenticationData(out AuthenticationData authenData)
        {
            _authenticationData = GetAuthenticationData();
            authenData = _authenticationData;
            if (authenData == null)
                return false;

            return true;
        }

        public void ClearAuthenticationData()
        {
            _authenticationData = null;
            PlayerPrefManager.SetEncryptedString(AuthenTokenKey, "");
        }
    }

    public class AuthenticateTokenExpiredException : Exception
    {
        public AuthenticateTokenExpiredException(IPublisher<ShowPopupSignal> publisher) : base()
        {
            ShowExpiredSessionPopup(publisher);
        }

        private void ShowExpiredSessionPopup(IPublisher<ShowPopupSignal> publisher)
        {
            publisher.Publish(new ShowPopupSignal(title: "Expired Error", noContent: "Okay", noAction: (_, _) => { }).SetInitialInput(new bool[] { true }, new string[] { "Your Token is Expired, Please Login and Try Again!" }));
        }
    }
}
