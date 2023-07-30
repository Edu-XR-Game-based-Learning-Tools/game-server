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
using UnityEngine;
using VContainer;

namespace Core.View
{
    public enum LandingScreenUserDropdownActionType
    {
        Logout
    }

    public enum ToolType
    {
        Quizzes
    }

    public class LandingScreenView : UnityView
    {
        private GameStore _gameStore;
        private AudioPoolManager _audioPoolManager;
        private UserAuthentication _userAuthentication;
        private ClassRoomHub _classRoomHub;
        private VirtualRoomPresenter _virtualRoomPresenter;
        private IUserDataController _userDataController;

        [SerializeField][DebugOnly] private MRTKTMPInputField _roomInputField;
        [SerializeField][DebugOnly] private PressableButton _joinBtn;
        [SerializeField][DebugOnly] private PressableButton _createBtn;

        [SerializeField][DebugOnly] private PressableButton _loginBtn;
        [SerializeField][DebugOnly] private PressableButton _userBtn;
        [SerializeField][DebugOnly] private Transform _userDropdown;
        [SerializeField][DebugOnly] private PressableButton[] _userDropdownActions;

        [SerializeField][DebugOnly] private PressableButton[] _toolBtns;
        [SerializeField][DebugOnly] private PressableButton[] _openToolBtns;

        [SerializeField][DebugOnly] private PressableButton _prevBtn;
        [SerializeField][DebugOnly] private PressableButton _nextBtn;

        [SerializeField][DebugOnly] private PressableButton _settingBtn;

        [Inject]
        public void Init(
            GameStore gameStore,
            IObjectResolver container)
        {
            _gameStore = gameStore;
            _audioPoolManager = (AudioPoolManager)container.Resolve<IReadOnlyList<IPoolManager>>().ElementAt((int)PoolName.Audio);
            _userAuthentication = container.Resolve<UserAuthentication>();
            _classRoomHub = container.Resolve<ClassRoomHub>();
            _virtualRoomPresenter = container.Resolve<VirtualRoomPresenter>();
            _userDataController = container.Resolve<IUserDataController>();
        }

        private void GetReferences()
        {
            _roomInputField = transform.Find("CanvasDialog/Canvas/Header/JoinRoom/InputField/InputField (TMP)").GetComponent<MRTKTMPInputField>();
            _joinBtn = transform.Find("CanvasDialog/Canvas/Header/JoinRoom/Join_Btn").GetComponent<PressableButton>();
            _createBtn = transform.Find("CanvasDialog/Canvas/Header/JoinRoom/Create_Btn").GetComponent<PressableButton>();

            _loginBtn = transform.Find("CanvasDialog/Canvas/Header/User/Login_Btn").GetComponent<PressableButton>();
            _userBtn = transform.Find("CanvasDialog/Canvas/Header/User/IsLoggedIn/User_Btn").GetComponent<PressableButton>();
            _userDropdown = transform.Find("CanvasDialog/Canvas/Header/User/IsLoggedIn/User_Dropdown");
            _userDropdownActions = new bool[_userDropdown.childCount].Select((_, idx) => _userDropdown.GetChild(idx).GetComponent<PressableButton>()).ToArray();

            var toolContent = transform.Find("CanvasDialog/Canvas/Content/Scroll View/Viewport/Content");
            _toolBtns = new bool[toolContent.childCount].Select((_, idx) => toolContent.GetChild(idx).GetComponent<PressableButton>()).ToArray();
            _openToolBtns = _toolBtns.Select(tool => tool.transform.Find("Frontplate/AnimatedContent/Header/Play_Btn").GetComponent<PressableButton>()).ToArray();

            _prevBtn = transform.Find("CanvasDialog/Canvas/Footer/NavButtons/Prev_Btn").GetComponent<PressableButton>();
            _nextBtn = transform.Find("CanvasDialog/Canvas/Footer/NavButtons/Next_Btn").GetComponent<PressableButton>();

            _settingBtn = transform.Find("CanvasDialog/Canvas/Footer/Setting_Btn").GetComponent<PressableButton>();

            _userBtn.transform.parent.SetActive(false);
            _userDropdown.SetActive(false);
        }

        private void EnableIsSignIn()
        {
            bool isSignIn = _userAuthentication.IsSignedIn;
            _loginBtn.SetActive(!isSignIn);
            _userBtn.transform.parent.SetActive(isSignIn);
            _createBtn.SetActive(isSignIn);
        }

        private async UniTask OnSuccessJoinRoom(RoomStatusResponse response)
        {
            _userDataController.ServerData.RoomStatus = new()
            {
                RoomStatus = response
            };

            _gameStore.GState.RemoveModel<LandingScreenModel>();
            (await _gameStore.GetOrCreateModule<RoomStatus, RoomStatusModel>(
                "", ViewName.Unity, ModuleName.RoomStatus)).Model.Refresh();

            await _virtualRoomPresenter.Spawn();
        }

        private void AskForPassword(string userName)
        {
            _showPopupPublisher.Publish(new ShowPopupSignal(title: "Room Password", yesContent: "Join", noContent: "Cancel", yesAction: async (value, _) =>
            {
                _showLoadingPublisher.Publish(new ShowLoadingSignal());
                RoomStatusResponse response = await _classRoomHub.JoinAsync(new JoinClassRoomData
                {
                    RoomId = _roomInputField.text,
                    UserName = userName,
                    Password = value,
                });

                if (_gameStore.CheckShowToastIfNotSuccessNetwork(response))
                    return;

                await OnSuccessJoinRoom(response);

                _showLoadingPublisher.Publish(new ShowLoadingSignal(isShow: false));
            }, noAction: (_, _) => { }).SetInitialInput(new bool[] { true }, new string[] { "Enter Password" }));
        }

        private void RegisterEvents()
        {
            _joinBtn.OnClicked.AddListener(() =>
            {
                _showPopupPublisher.Publish(new ShowPopupSignal(title: "Enter Your Name", yesContent: "Join", noContent: "Cancel", yesAction: async (value, _) =>
                {
                    _showLoadingPublisher.Publish(new ShowLoadingSignal());
                    RoomStatusResponse response = await _classRoomHub.JoinAsync(new JoinClassRoomData
                    {
                        RoomId = _roomInputField.text,
                        UserName = value
                    });

                    if (!response.Success && !string.IsNullOrEmpty(response.Password))
                        AskForPassword(value);
                    else if (_gameStore.CheckShowToastIfNotSuccessNetwork(response))
                        return;

                    await OnSuccessJoinRoom(response);

                    _showLoadingPublisher.Publish(new ShowLoadingSignal(isShow: false));
                }, noAction: (_, _) => { }).SetInitialInput(new bool[] { true }, new string[] { "Enter name" }));
            });

            _createBtn.OnClicked.AddListener(() =>
            {
                _showPopupPublisher.Publish(new ShowPopupSignal(title: "Room Configuration", yesContent: "Create", noContent: "Cancel", yesAction: async (value1, value2) =>
                {
                    _showLoadingPublisher.Publish(new ShowLoadingSignal());
                    RoomStatusResponse response = await _classRoomHub.JoinAsync(new JoinClassRoomData()
                    {
                        Password = value1,
                        Amount = int.Parse(value2),
                    });

                    if (_gameStore.CheckShowToastIfNotSuccessNetwork(response))
                        return;

                    await OnSuccessJoinRoom(response);

                    _showLoadingPublisher.Publish(new ShowLoadingSignal(isShow: false));
                }, noAction: (_, _) => { }).SetInitialInput(new bool[] { true, true }, new string[] { "Enter Password", "Enter Number of Slot" }));

            });

            _loginBtn.OnClicked.AddListener(async () =>
            {
                _gameStore.GState.RemoveModel<LandingScreenModel>();
                await _gameStore.GetOrCreateModule<LoginScreen, LoginScreenModel>(
                    "", ViewName.Unity, ModuleName.LoginScreen);
            });
            _userBtn.OnClicked.AddListener(() =>
            {
                _userDropdown.SetActive(!_userDropdown.gameObject.activeInHierarchy);
            });
            for (int idx = 0; idx < _userDropdownActions.Length; idx++)
            {
                int index = idx;
                _userDropdownActions[idx].OnClicked.AddListener(() =>
                {
                    switch ((LandingScreenUserDropdownActionType)index)
                    {
                        case LandingScreenUserDropdownActionType.Logout:
                            _userAuthentication.ClearAuthenticationData();
                            EnableIsSignIn();
                            break;

                        default:
                            Debug.Log(CoreDefines.NotAvailable);
                            break;
                    }
                });
            }

            for (int idx = 0; idx < _toolBtns.Length; idx++)
            {
                _toolBtns[idx].OnClicked.AddListener(async () =>
                {
                    _gameStore.GState.RemoveModel<LandingScreenModel>();
                    await _gameStore.GetOrCreateModule<ToolDescription, ToolDescriptionModel>(
                        "", ViewName.Unity, ModuleName.ToolDescription);
                });
            }
            for (int idx = 0; idx < _openToolBtns.Length; idx++)
            {
                _openToolBtns[idx].OnClicked.AddListener(async () =>
                {
                    _gameStore.GState.RemoveModel<LandingScreenModel>();
                    await _gameStore.GetOrCreateModule<RoomStatus, RoomStatusModel>(
                        "", ViewName.Unity, ModuleName.RoomStatus);
                });
            }

            _prevBtn.OnClicked.AddListener(() =>
            {
                Debug.Log(CoreDefines.NotAvailable);
            });
            _nextBtn.OnClicked.AddListener(() =>
            {
                Debug.Log(CoreDefines.NotAvailable);
            });

            _settingBtn.OnClicked.AddListener(async () =>
            {
                _gameStore.GState.RemoveModel<LandingScreenModel>();
                await _gameStore.GetOrCreateModule<SettingScreen, SettingScreenModel>(
                    "", ViewName.Unity, ModuleName.SettingScreen);
            });
        }

        public override void OnReady()
        {
            GetReferences();
            RegisterEvents();

            Refresh();
        }

        public void Refresh()
        {
            bool isInGameView = _userDataController.ServerData.IsInGame;
            _roomInputField.SetActive(!isInGameView);
            _joinBtn.SetActive(!isInGameView);
            _createBtn.SetActive(!isInGameView);
            _userDropdown.GetChild((int)LandingScreenUserDropdownActionType.Logout).SetActive(!isInGameView);
            foreach (var btn in _openToolBtns)
                btn.SetActive(!isInGameView);

            EnableIsSignIn();
        }
    }
}
