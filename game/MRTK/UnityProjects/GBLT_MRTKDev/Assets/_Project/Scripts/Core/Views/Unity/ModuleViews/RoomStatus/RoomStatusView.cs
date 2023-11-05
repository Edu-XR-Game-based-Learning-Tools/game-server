using Core.Business;
using Core.EventSignal;
using Core.Extension;
using Core.Framework;
using Core.Module;
using Core.Utility;
using Cysharp.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.UX;
using Shared;
using Shared.Extension;
using Shared.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Core.View
{
    [System.Serializable]
    public class RoomStatusPerson
    {
        [DebugOnly] public PressableButton Button;
        [DebugOnly] public TextMeshProUGUI NameTxt;
        [DebugOnly] public Image IconImg;
    }

    [System.Serializable]
    public abstract class SubView
    {
        [DebugOnly] public Transform Transform;
        [SerializeField][DebugOnly] protected PressableButton _backBtn;
        [SerializeField][DebugOnly] protected IObjectResolver _container;
        [SerializeField][DebugOnly] protected Transform _viewRoot;
        [SerializeField][DebugOnly] protected Action _onBack;

        public SubView(Transform transform, IObjectResolver container, Transform viewRoot, Action onBack = null)
        {
            Transform = transform;
            _container = container;
            _viewRoot = viewRoot;
            _onBack = onBack;
        }

        public virtual void RegisterEvents()
        {
            _backBtn.OnClicked.AddListener(() =>
            {
                _onBack?.Invoke();
                Transform.SetActive(false);
            });
        }
    }

    public class RoomStatusView : UnityView
    {
        [System.Serializable]
        public class SelectToolView : SubView
        {
            private readonly GameStore _gameStore;
            private readonly ClassRoomHub _classRoomHub;
            private readonly QuizzesHub _quizzesHub;
            private readonly IUserDataController _userDataController;

            [SerializeField][DebugOnly] protected PressableButton[] _toolBtns;

            public SelectToolView(Transform transform, IObjectResolver container, Transform viewRoot, Action onBack) : base(transform, container, viewRoot, onBack)
            {
                _gameStore = container.Resolve<GameStore>();
                _classRoomHub = container.Resolve<ClassRoomHub>();
                _quizzesHub = container.Resolve<QuizzesHub>();
                _userDataController = container.Resolve<IUserDataController>();

                _backBtn = Transform.Find("Content/Header/Back_Btn").GetComponent<PressableButton>();

                _toolBtns = new PressableButton[] {
                    Transform.Find("Content/Scroll View/Viewport/Content").GetChild(0).GetComponent<PressableButton>()
                };

                RegisterEvents();
            }

            public override void RegisterEvents()
            {
                base.RegisterEvents();

                _toolBtns[0].OnClicked.AddListener(async () =>
                {
                    QuizzesStatusResponse response = await _quizzesHub.JoinAsync(new JoinQuizzesData());
                    await _classRoomHub.InviteToGame(response);

                    if (_gameStore.CheckShowToastIfNotSuccessNetwork(response))
                        return;

                    _userDataController.ServerData.RoomStatus.InGameStatus = response;

                    _gameStore.GState.RemoveModel<RoomStatusModel>();
                    await _gameStore.GetOrCreateModule<QuizzesRoomStatus, QuizzesRoomStatusModel>(
                        "", ViewName.Unity, ModuleName.QuizzesRoomStatus);
                });
            }
        }

        [System.Serializable]
        public class EditAvatarView : SubView
        {
            private readonly IBundleLoader _bundleLoader;
            private readonly ClassRoomHub _classRoomHub;
            private IUserDataController _userDataController;

            [SerializeField][DebugOnly] protected MRTKTMPInputField _nameInput;
            [SerializeField][DebugOnly] protected PressableButton[] _avatarBtns;
            [SerializeField][DebugOnly] protected PressableButton[] _modelBtns;
            [SerializeField][DebugOnly] protected PressableButton _submitBtn;

            public string SelectedAvatarPath => Defines.PrefabKey.AvatarPaths[_selectedAvatarIdx];
            [SerializeField][DebugOnly] protected int _selectedAvatarIdx = 0;
            public string SelectedModelPath => Defines.PrefabKey.ModelPaths[_selectedModelIdx];
            [SerializeField][DebugOnly] protected int _selectedModelIdx = 0;

            [SerializeField] protected Color _selectedColor = new(1f, 1f, 1f, 1f);
            [SerializeField] protected Color _defaultColor = "#27984CFF".HexToColor();

            public EditAvatarView(Transform transform, IObjectResolver container, Transform viewRoot, Action onBack) : base(transform, container, viewRoot, onBack)
            {
                _classRoomHub = container.Resolve<ClassRoomHub>();
                _bundleLoader = container.Resolve<IReadOnlyList<IBundleLoader>>().ElementAt((int)BundleLoaderName.Addressable);
                _userDataController = container.Resolve<IUserDataController>();

                _backBtn = Transform.Find("Content/Header/Back_Btn").GetComponent<PressableButton>();

                _nameInput = Transform.Find("Content/Form/InputField/InputField (TMP)").GetComponent<MRTKTMPInputField>();

                Transform parent = Transform.Find("Content/Form/Avatar_SV/Viewport/Content");
                _avatarBtns = Defines.PrefabKey.AvatarPaths.Select((path, idx) =>
                {
                    if (idx > 0)
                        Instantiate(parent.GetChild(0), parent);
                    parent.GetChild(idx).name = $"{idx} - {Defines.PrefabKey.AvatarPaths[idx]}";
                    return parent.GetChild(idx).GetComponent<PressableButton>();
                }).ToArray();

                parent = Transform.Find("Content/Form/Model_SV/Viewport/Content");
                _modelBtns = Defines.PrefabKey.ModelThumbnailPaths.Select((path, idx) =>
                {
                    if (idx > 0)
                        Instantiate(parent.GetChild(0), parent);
                    parent.GetChild(idx).name = $"{idx} - {Defines.PrefabKey.ModelThumbnailPaths[idx]}";
                    return parent.GetChild(idx).GetComponent<PressableButton>();
                }).ToArray();

                _submitBtn = Transform.Find("Content/Form/Submit_Btn").GetComponent<PressableButton>();

                SetAvatarIcon();

                RegisterEvents();
            }

            private void EnableAvatar(int index)
            {
                _avatarBtns[_selectedAvatarIdx].transform.Find("Backplate").GetComponent<Image>().color = _defaultColor;
                _selectedAvatarIdx = index;
                _avatarBtns[_selectedAvatarIdx].transform.Find("Backplate").GetComponent<Image>().color = _selectedColor;
            }

            private void EnableModel(int index)
            {
                _modelBtns[_selectedModelIdx].transform.Find("Backplate").GetComponent<Image>().color = _defaultColor;
                _selectedModelIdx = index;
                _modelBtns[_selectedModelIdx].transform.Find("Backplate").GetComponent<Image>().color = _selectedColor;
            }

            private async void SetAvatarIcon()
            {
                for (int idx = 0; idx < _avatarBtns.Length; idx++)
                {
                    _avatarBtns[idx].transform.Find("Frontplate/AnimatedContent/Icon/UIButtonSpriteIcon").GetComponent<Image>().sprite = await ((UserDataController)_userDataController).LocalUserCache.GetSprite(Defines.PrefabKey.AvatarPaths[idx]);

                    _avatarBtns[idx].transform.Find("Backplate").GetComponent<Image>().color = _defaultColor;
                }

                for (int idx = 0; idx < _modelBtns.Length; idx++)
                {
                    _modelBtns[idx].transform.Find("Frontplate/AnimatedContent/Icon/UIButtonSpriteIcon").GetComponent<Image>().sprite = await ((UserDataController)_userDataController).LocalUserCache.GetSprite(Defines.PrefabKey.ModelThumbnailPaths[idx]);

                    _modelBtns[idx].transform.Find("Backplate").GetComponent<Image>().color = _defaultColor;
                }

                _selectedAvatarIdx = Defines.PrefabKey.AvatarPaths.Select((path, idx) => (path, idx)).Where(ele => ele.path == _userDataController.ServerData.RoomStatus.RoomStatus.Self.AvatarPath).First().idx;
                EnableAvatar(_selectedAvatarIdx);

                _selectedModelIdx = Defines.PrefabKey.ModelPaths.Select((path, idx) => (path, idx)).Where(ele => ele.path == _userDataController.ServerData.RoomStatus.RoomStatus.Self.ModelPath).First().idx;
                EnableModel(_selectedModelIdx);
            }

            public override void RegisterEvents()
            {
                base.RegisterEvents();

                for (int idx = 0; idx < _avatarBtns.Length; idx++)
                {
                    int index = idx;
                    _avatarBtns[idx].OnClicked.AddListener(() => EnableAvatar(index));
                }

                bool test = false;
                for (int idx = 0; idx < _modelBtns.Length; idx++)
                {
                    int index = idx;
                    _modelBtns[idx].OnClicked.AddListener(() => EnableModel(index));
                }

                _submitBtn.OnClicked.AddListener(async () =>
                {
                    await _classRoomHub.UpdateAvatar(_nameInput.text.IsNullOrEmpty() ? "Name" : _nameInput.text, SelectedModelPath, SelectedAvatarPath);
                    Transform.SetActive(false);
                    _onBack?.Invoke();
                });
            }
        }

        [System.Serializable]
        public class SettingView : SubView
        {
            [SerializeField][DebugOnly] protected MRTKTMPInputField _roomCapInput;

            public SettingView(Transform transform, IObjectResolver container, Transform viewRoot, Action onBack) : base(transform, container, viewRoot, onBack)
            {
                _backBtn = Transform.Find("Content/Header/Back_Btn").GetComponent<PressableButton>();

                _roomCapInput = Transform.Find("Content/Form/InputField/InputField (TMP)").GetComponent<MRTKTMPInputField>();

                RegisterEvents();
            }

            public override void RegisterEvents()
            {
                base.RegisterEvents();
            }
        }

        private IObjectResolver _container;
        private GameStore _gameStore;
        private AudioPoolManager _audioPoolManager;
        private VirtualRoomPresenter _virtualRoomPresenter;
        private IUserDataController _userDataController;
        private ClassRoomHub _classRoomHub;

        [SerializeField][DebugOnly] private PressableButton _closeBtn;
        [SerializeField][DebugOnly] private PressableButton _quitBtn;

        [SerializeField][DebugOnly] private TextMeshProUGUI _titleTxt;
        [SerializeField][DebugOnly] private TextMeshProUGUI _amountTxt;
        [SerializeField][DebugOnly] private RoomStatusPerson _hostItem;
        [SerializeField][DebugOnly] private RoomStatusPerson[] _personItems;

        [SerializeField][DebugOnly] private PressableButton _selectToolBtn;
        [SerializeField][DebugOnly] private PressableButton _editAvatarBtn;
        [SerializeField][DebugOnly] private PressableButton _settingBtn;

        [DebugOnly] public Transform RootContent;
        [SerializeField][DebugOnly] private EditAvatarView _editAvatarView;
        [SerializeField][DebugOnly] private SelectToolView _selectToolView;
        [SerializeField][DebugOnly] private SettingView _settingView;

        [Inject]
        public void Init(
            GameStore gameStore,
            IObjectResolver container)
        {
            _container = container;
            _gameStore = gameStore;
            _audioPoolManager = (AudioPoolManager)container.Resolve<IReadOnlyList<IPoolManager>>().ElementAt((int)PoolName.Audio);
            _virtualRoomPresenter = container.Resolve<VirtualRoomPresenter>();
            _userDataController = container.Resolve<IUserDataController>();
            _classRoomHub = container.Resolve<ClassRoomHub>();
        }

        private void GetReferences()
        {
            _closeBtn = transform.Find("CanvasDialog/Canvas/Content/Header/Close_Btn").GetComponent<PressableButton>();
            _quitBtn = transform.Find("CanvasDialog/Canvas/Content/Header/Quit_Btn").GetComponent<PressableButton>();

            _titleTxt = transform.Find("CanvasDialog/Canvas/Content/Header/Content/Title").GetComponent<TextMeshProUGUI>();
            _amountTxt = transform.Find("CanvasDialog/Canvas/Content/Header/Content/Amount").GetComponent<TextMeshProUGUI>();

            var hostTransform = transform.Find("CanvasDialog/Canvas/Content/Content/Person");
            _hostItem = new RoomStatusPerson
            {
                Button = hostTransform.GetComponent<PressableButton>(),
                NameTxt = hostTransform.Find("Frontplate/AnimatedContent/Text").GetComponent<TextMeshProUGUI>(),
                IconImg = hostTransform.Find("Frontplate/AnimatedContent/Icon/UIButtonSpriteIcon").GetComponent<Image>(),
            };
            var list = transform.Find("CanvasDialog/Canvas/Content/Content/Scroll View/Viewport/Content");
            _personItems = new bool[list.childCount].Select((_, idx) =>
            {
                Transform person = list.GetChild(idx);
                return new RoomStatusPerson
                {
                    Button = person.GetComponent<PressableButton>(),
                    NameTxt = person.Find("Frontplate/AnimatedContent/Text").GetComponent<TextMeshProUGUI>(),
                    IconImg = person.Find("Frontplate/AnimatedContent/Icon/UIButtonSpriteIcon").GetComponent<Image>(),
                };
            }).ToArray();

            _selectToolBtn = transform.Find("CanvasDialog/Canvas/Content/Footer/SelectTool_Btn").GetComponent<PressableButton>();
            _editAvatarBtn = transform.Find("CanvasDialog/Canvas/Content/Footer/EditAvatar_Btn").GetComponent<PressableButton>();
            _settingBtn = transform.Find("CanvasDialog/Canvas/Content/Footer/Setting_Btn").GetComponent<PressableButton>();

            RootContent = transform.Find("CanvasDialog/Canvas/Content");
            _selectToolView = new SelectToolView(transform.Find("CanvasDialog/Canvas/ToolSelection"), _container, transform, OnBack);
            _editAvatarView = new EditAvatarView(transform.Find("CanvasDialog/Canvas/EditAvatar"), _container, transform, OnBack);
            _settingView = new SettingView(transform.Find("CanvasDialog/Canvas/Setting"), _container, transform, OnBack);

            _selectToolView.Transform.SetActive(false);
            _editAvatarView.Transform.SetActive(false);
            _settingView.Transform.SetActive(false);
        }

        private void OnBack()
        {
            RootContent.SetActive(true);
        }

        private void RegisterEvents()
        {
            _closeBtn.OnClicked.AddListener(() =>
            {
                _gameStore.HideCurrentModule(ModuleName.RoomStatus);
            });
            _quitBtn.OnClicked.AddListener(() =>
            {
                _showPopupPublisher.Publish(new ShowPopupSignal(title: "Are you sure you want to quit the class room?", yesContent: "Yes", noContent: "No", yesAction: async (value1, value2) =>
                {
                    _showLoadingPublisher.Publish(new ShowLoadingSignal());
                    await _classRoomHub.LeaveAsync();

                    _gameStore.GState.RemoveModel<RoomStatusModel>();
                    await _gameStore.GetOrCreateModule<LandingScreen, LandingScreenModel>(
                        "", ViewName.Unity, ModuleName.LandingScreen);

                    _showLoadingPublisher.Publish(new ShowLoadingSignal(isShow: false));
                }, noAction: (_, _) => { }));
            });

            for (int idx = 0; idx < _personItems.Length; idx++)
            {
                int index = idx;
                _personItems[index].Button.OnClicked.AddListener(() =>
                {
                    Debug.Log($"{_personItems[index].NameTxt.text} - {index}");
                });
            }

            _selectToolBtn.OnClicked.AddListener(() =>
            {
                RootContent.SetActive(false);
                _selectToolView.Transform.SetActive(true);
            });
            _editAvatarBtn.OnClicked.AddListener(() =>
            {
                RootContent.SetActive(false);
                _editAvatarView.Transform.SetActive(true);
            });
            _settingBtn.OnClicked.AddListener(() =>
            {
                RootContent.SetActive(false);
                _settingView.Transform.SetActive(true);
            });
        }

        public override void OnReady()
        {
            GetReferences();
            RegisterEvents();

            Refresh();
        }

        public async UniTask UpdateCharacter(PublicUserData userData, bool isShow = true)
        {
            if (userData.IsHost)
                _hostItem.Button.SetActive(isShow);
            else
                _personItems[userData.Index].Button.SetActive(isShow);
            if (!isShow) return;

            var avt = await ((UserDataController)_userDataController).LocalUserCache.GetSprite(userData.AvatarPath);
            if (userData.IsHost)
            {
                _hostItem.NameTxt.text = userData.Name;
                _hostItem.IconImg.sprite = avt;
                return;
            }

            _personItems[userData.Index].NameTxt.text = userData.Name;
            _personItems[userData.Index].IconImg.sprite = avt;
        }

        public async void Refresh()
        {
            if (!_userDataController.ServerData.IsInRoom)
            {
                _gameStore.GState.RemoveModel<RoomStatusModel>();
                return;
            }

            bool isInGame = _userDataController.ServerData.IsInGame;
            GeneralRoomStatusResponse status = _userDataController.ServerData.RoomStatus.RoomStatus;

            string idPrefix = isInGame ? "PIN" : "Room Id";
            _titleTxt.SetText($"{idPrefix}: {status.Id}");
            _amountTxt.SetText($"Amount: {status.Amount}");
            for (int idx = 0; idx < _personItems.Length; idx++)
                _personItems[idx].Button.SetActive(false);
            for (int idx = 0; idx < status.AllInRoom.Length; idx++)
                await UpdateCharacter(status.AllInRoom[idx]);

            bool isHost = _userDataController.ServerData.RoomStatus.RoomStatus.Self.IsHost;
            _selectToolBtn.SetActive(isHost && !isInGame);
            _editAvatarBtn.SetActive(!isInGame);
            _settingBtn.SetActive(isHost && !isInGame);
        }
    }
}
