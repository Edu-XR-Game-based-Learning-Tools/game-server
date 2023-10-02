using Core.Business;
using Core.Extension;
using Core.Framework;
using Core.Module;
using Core.Utility;
using Microsoft.MixedReality.Toolkit.UX;
using Shared.Extension;
using Shared.Network;
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

        public SubView(Transform transform)
        {
            Transform = transform;
        }

        public virtual void RegisterEvents()
        { }
    }

    [System.Serializable]
    public class EditAvatarView : SubView
    {
        [SerializeField][DebugOnly] protected MRTKTMPInputField _nameInput;
        [SerializeField][DebugOnly] protected PressableButton _avatarBtn;
        [SerializeField][DebugOnly] protected Image _avatarImg;

        public EditAvatarView(Transform transform) : base(transform)
        {
            _backBtn = Transform.Find("Content/Header/Back_Btn").GetComponent<PressableButton>();

            _nameInput = Transform.Find("Content/Form/InputField/InputField (TMP)").GetComponent<MRTKTMPInputField>();
            _avatarBtn = Transform.Find("Content/Form/Person").GetComponent<PressableButton>();
            _avatarImg = Transform.Find("Content/Form/Person/Frontplate/AnimatedContent/Icon/UIButtonSpriteIcon").GetComponent<Image>();

            RegisterEvents();
        }

        public override void RegisterEvents()
        {
            _backBtn.OnClicked.AddListener(() => Transform.SetActive(false));
        }
    }

    public class RoomStatusView : UnityView
    {
        [System.Serializable]
        public class SelectQuizView : SubView
        {
            [SerializeField][DebugOnly] protected PressableButton[] _toolBtns;

            public SelectQuizView(Transform transform) : base(transform)
            {
                _backBtn = Transform.Find("Content/Header/Back_Btn").GetComponent<PressableButton>();

                _toolBtns = new PressableButton[] {
                    Transform.Find("Content/Scroll View/Viewport/Content").GetChild(0).GetComponent<PressableButton>()
                };

                RegisterEvents();
            }

            public override void RegisterEvents()
            {
                _backBtn.OnClicked.AddListener(() => Transform.SetActive(false));
            }
        }

        [System.Serializable]
        public class SettingView : SubView
        {
            [SerializeField][DebugOnly] protected MRTKTMPInputField _roomCapInput;

            public SettingView(Transform transform) : base(transform)
            {
                _backBtn = Transform.Find("Content/Header/Back_Btn").GetComponent<PressableButton>();

                _roomCapInput = Transform.Find("Content/Form/InputField/InputField (TMP)").GetComponent<MRTKTMPInputField>();

                RegisterEvents();
            }

            public override void RegisterEvents()
            {
                _backBtn.OnClicked.AddListener(() => Transform.SetActive(false));
            }
        }

        private GameStore _gameStore;
        private AudioPoolManager _audioPoolManager;
        private VirtualRoomPresenter _virtualRoomPresenter;
        private IUserDataController _userDataController;

        [SerializeField][DebugOnly] private PressableButton _closeBtn;
        [SerializeField][DebugOnly] private PressableButton _backBtn;

        [SerializeField][DebugOnly] private TextMeshProUGUI _titleTxt;
        [SerializeField][DebugOnly] private TextMeshProUGUI _amountTxt;
        [SerializeField][DebugOnly] private RoomStatusPerson[] _personItems;

        [SerializeField][DebugOnly] private PressableButton _selectQuizBtn;
        [SerializeField][DebugOnly] private PressableButton _editAvatarBtn;
        [SerializeField][DebugOnly] private PressableButton _settingBtn;

        [SerializeField][DebugOnly] private EditAvatarView _editAvatarView;
        [SerializeField][DebugOnly] private SelectQuizView _selectQuizView;
        [SerializeField][DebugOnly] private SettingView _settingView;

        [Inject]
        public void Init(
            GameStore gameStore,
            IObjectResolver container)
        {
            _gameStore = gameStore;
            _audioPoolManager = (AudioPoolManager)container.Resolve<IReadOnlyList<IPoolManager>>().ElementAt((int)PoolName.Audio);
            _virtualRoomPresenter = container.Resolve<VirtualRoomPresenter>();
            _userDataController = container.Resolve<IUserDataController>();
        }

        private void GetReferences()
        {
            _closeBtn = transform.Find("CanvasDialog/Canvas/Header/Close_Btn").GetComponent<PressableButton>();
            _backBtn = transform.Find("CanvasDialog/Canvas/Header/Back_Btn").GetComponent<PressableButton>();

            _titleTxt = transform.Find("CanvasDialog/Canvas/Header/Content/Title").GetComponent<TextMeshProUGUI>();
            _amountTxt = transform.Find("CanvasDialog/Canvas/Header/Content/Amount").GetComponent<TextMeshProUGUI>();
            var list = transform.Find("CanvasDialog/Canvas/Content/Scroll View/Viewport/Content");
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

            _selectQuizBtn = transform.Find("CanvasDialog/Canvas/Footer/SelectQuiz_Btn").GetComponent<PressableButton>();
            _editAvatarBtn = transform.Find("CanvasDialog/Canvas/Footer/EditAvatar_Btn").GetComponent<PressableButton>();
            _settingBtn = transform.Find("CanvasDialog/Canvas/Footer/Setting_Btn").GetComponent<PressableButton>();

            _selectQuizView = new SelectQuizView(transform.Find("CanvasDialog/Canvas/ToolSelection"));
            _editAvatarView = new EditAvatarView(transform.Find("CanvasDialog/Canvas/EditAvatar"));
            _settingView = new SettingView(transform.Find("CanvasDialog/Canvas/Setting"));

            _selectQuizView.Transform.SetActive(false);
            _editAvatarView.Transform.SetActive(false);
            _settingView.Transform.SetActive(false);
        }

        private void RegisterEvents()
        {
            _closeBtn.OnClicked.AddListener(() =>
            {
                _gameStore.HideCurrentModule(ModuleName.RoomStatus);
            });
            _backBtn.OnClicked.AddListener(async () =>
            {
                _gameStore.GState.RemoveModel<RoomStatusModel>();
                await _gameStore.GetOrCreateModule<LandingScreen, LandingScreenModel>(
                    "", ViewName.Unity, ModuleName.LandingScreen);
            });

            for (int idx = 0; idx < _personItems.Length; idx++)
            {
                int index = idx;
                _personItems[index].Button.OnClicked.AddListener(() =>
                {
                    Debug.Log($"{_personItems[index].NameTxt.text} - {index}");
                });
            }

            _selectQuizBtn.OnClicked.AddListener(() =>
                _selectQuizView.Transform.SetActive(true));
            _editAvatarBtn.OnClicked.AddListener(() =>
                _editAvatarView.Transform.SetActive(true));
            _selectQuizBtn.OnClicked.AddListener(() =>
                _settingView.Transform.SetActive(true));
        }

        public override void OnReady()
        {
            GetReferences();
            RegisterEvents();

            Refresh();
        }

        public void Refresh()
        {
            if (!_userDataController.ServerData.IsInRoom)
            {
                _gameStore.GState.RemoveModel<RoomStatusModel>();
                return;
            }

            bool isInGame = _userDataController.ServerData.IsInGame;
            GeneralRoomStatusResponse status = isInGame ? _userDataController.ServerData.RoomStatus.InGameStatus : _userDataController.ServerData.RoomStatus.RoomStatus;

            string idPrefix = isInGame ? "PIN" : "Room Id";
            _titleTxt.SetText($"{idPrefix}: {status.Id}");
            _amountTxt.SetText($"Amount: {status.Amount}");
            for (int idx = 0; idx < _personItems.Length; idx++)
            {
                _personItems[idx].Button.SetActive(idx >= status.Others.Length);
                if (idx >= status.Others.Length) continue;

                if (status.Others[idx].IsHost)
                {
                    idx--;
                    continue;
                }

                _personItems[idx].NameTxt.text = status.Others[idx].Name;
            }

            bool isHost = _userDataController.ServerData.RoomStatus.RoomStatus.Self.IsHost;
            _selectQuizBtn.SetActive(isHost && !isInGame);
            _editAvatarBtn.SetActive(isHost && !isInGame);
            _settingBtn.SetActive(isHost && !isInGame);
        }
    }
}
