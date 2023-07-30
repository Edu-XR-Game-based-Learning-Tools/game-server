using Core.Business;
using Core.EventSignal;
using Core.Extension;
using Core.Framework;
using Core.Module;
using Core.Network;
using Core.Utility;
using Cysharp.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.UX;
using Shared.Network;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using VContainer;

namespace Core.View
{
    public class LoginScreenView : UnityView
    {
        private GameStore _gameStore;
        private AudioPoolManager _audioPoolManager;
        private IRpcAuthController _rpcAuthController;
        private UserAuthentication _userAuthentication;

        [SerializeField][DebugOnly] private MRTKTMPInputField _usernameInput;
        [SerializeField][DebugOnly] private Transform _emailTip;
        [SerializeField][DebugOnly] private Transform _emailInputContainer;
        [SerializeField][DebugOnly] private MRTKTMPInputField _emailInput;
        [SerializeField][DebugOnly] private MRTKTMPInputField _passwordInput;
        [SerializeField][DebugOnly] private Transform _rePasswordTip;
        [SerializeField][DebugOnly] private Transform _rePasswordInputContainer;
        [SerializeField][DebugOnly] private MRTKTMPInputField _rePasswordInput;

        [SerializeField][DebugOnly] private TextMeshProUGUI _loginTxt;
        [SerializeField][DebugOnly] private PressableButton _loginBtn;
        [SerializeField][DebugOnly] private TextMeshProUGUI _formSwitchTxt;
        [SerializeField][DebugOnly] private TextMeshProUGUI _formSwitchBtnTxt;
        [SerializeField][DebugOnly] private PressableButton _formSwitchBtn;

        private bool _isShowLogin = true;

        [Inject]
        public void Init(
            GameStore gameStore,
            IObjectResolver container)
        {
            _gameStore = gameStore;
            _audioPoolManager = (AudioPoolManager)container.Resolve<IReadOnlyList<IPoolManager>>().ElementAt((int)PoolName.Audio);
            _rpcAuthController = container.Resolve<IRpcAuthController>();
            _userAuthentication = container.Resolve<UserAuthentication>();
        }

        private void GetReferences()
        {
            _usernameInput = transform.Find("CanvasDialog/Canvas/Content/Username_Input/InputField (TMP)").GetComponent<MRTKTMPInputField>();
            _emailTip = transform.Find("CanvasDialog/Canvas/Content/Email_Txt");
            _emailInputContainer = transform.Find("CanvasDialog/Canvas/Content/Email_Input");
            _emailInput = _emailInputContainer.transform.Find("InputField (TMP)").GetComponent<MRTKTMPInputField>();
            _passwordInput = transform.Find("CanvasDialog/Canvas/Content/Password_Input/InputField (TMP)").GetComponent<MRTKTMPInputField>();
            _rePasswordTip = transform.Find("CanvasDialog/Canvas/Content/RePassword_Txt");
            _rePasswordInputContainer = transform.Find("CanvasDialog/Canvas/Content/RePassword_Input");
            _rePasswordInput = _rePasswordInputContainer.transform.Find("InputField (TMP)").GetComponent<MRTKTMPInputField>();

            _loginBtn = transform.Find("CanvasDialog/Canvas/Footer/Login_Btn").GetComponent<PressableButton>();
            _loginTxt = _loginBtn.transform.Find("Frontplate/AnimatedContent/Text").GetComponent<TextMeshProUGUI>();
            _formSwitchTxt = transform.Find("CanvasDialog/Canvas/Footer/FormSwitcher/Text").GetComponent<TextMeshProUGUI>();
            _formSwitchBtn = transform.Find("CanvasDialog/Canvas/Footer/FormSwitcher/FormSwitch_Btn").GetComponent<PressableButton>();
            _formSwitchBtnTxt = _formSwitchBtn.transform.Find("Frontplate/AnimatedContent/Text").GetComponent<TextMeshProUGUI>();
        }

        private void EnableFormType(bool isShowLogin = true)
        {
            _isShowLogin = isShowLogin;

            _emailTip.SetActive(!_isShowLogin);
            _emailInputContainer.SetActive(!_isShowLogin);

            _rePasswordTip.SetActive(!_isShowLogin);
            _rePasswordInputContainer.SetActive(!_isShowLogin);

            _formSwitchTxt.text = _isShowLogin ? CoreDefines.DontHaveAccountYet : CoreDefines.AlreadyHaveAccount;
            _formSwitchBtnTxt.text = _isShowLogin ? CoreDefines.SignUp : CoreDefines.SignIn;

            _loginTxt.text = _isShowLogin ? CoreDefines.Login : CoreDefines.Register;
        }

        private async UniTask<AuthenticationData> HandleLogin()
        {
            AuthenticationData data = await _rpcAuthController.Login(new LoginRequest
            {
                Username = _usernameInput.text,
                Password = _passwordInput.text
            });

            return data;
        }

        private async UniTask<AuthenticationData> HandleRegister()
        {
            AuthenticationData data = await _rpcAuthController.Register(new RegisterRequest
            {
                Username = _usernameInput.text,
                Email = _emailInput.text,
                Password = _passwordInput.text,
                RePassword = _rePasswordInput.text,
            });

            return data;
        }

        private void RegisterEvents()
        {
            _loginBtn.OnClicked.AddListener(async () =>
            {
                AuthenticationData data;
                if (_isShowLogin)
                    data = await HandleLogin();
                else data = await HandleRegister();

                if (!data.Success)
                {
                    _showToastPublisher.Publish(new ShowToastSignal(content: data.Message)
                        .SetOnClose(true, () => { }));
                    Debug.Log($"{GetType()}: {data.Message}");
                    return;
                }

                _userAuthentication.Update(data);

                _gameStore.GState.RemoveModel<LoginScreenModel>();
                await _gameStore.GetOrCreateModule<LandingScreen, LandingScreenModel>(
                    "", ViewName.Unity, ModuleName.LandingScreen);
            });

            _formSwitchBtn.OnClicked.AddListener(() =>
            {
                EnableFormType(!_isShowLogin);
            });
        }

        public override void OnReady()
        {
            GetReferences();
            RegisterEvents();

            EnableFormType();
        }

        public void Refresh()
        {
        }
    }
}
