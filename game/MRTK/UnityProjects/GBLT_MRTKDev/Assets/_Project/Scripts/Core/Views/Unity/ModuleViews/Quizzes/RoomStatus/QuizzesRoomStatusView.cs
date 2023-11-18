using Core.Business;
using Core.EventSignal;
using Core.Extension;
using Core.Framework;
using Core.Module;
using Core.Utility;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Microsoft.MixedReality.Toolkit.UX;
using Shared.Extension;
using Shared.Network;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Core.View
{
    [System.Serializable]
    public class QuizzesRoomStatusPerson
    {
        [DebugOnly] public PressableButton Button;
        [DebugOnly] public TextMeshProUGUI NameTxt;
        [DebugOnly] public Image IconImg;
    }

    public class QuizzesRoomStatusView : UnityView
    {
        [System.Serializable]
        public class SelectQuizView : SubView
        {
            [SerializeField][DebugOnly] protected List<PressableButton> _quizBtns = new();
            [SerializeField][DebugOnly] protected QuizzesRoomStatusView _root;

            public SelectQuizView(Transform transform, IObjectResolver container, Transform viewRoot, System.Action onBack = null) : base(transform, container, viewRoot, onBack)
            {
                _backBtn = Transform.Find("Content/Header/Back_Btn").GetComponent<PressableButton>();

                _quizBtns.Add(
                    Transform.Find("Content/Scroll View/Viewport/Content").GetChild(0).GetComponent<PressableButton>());
            }

            public async UniTask SetupQuizzes(QuizCollectionDto[] collections, QuizzesRoomStatusView root)
            {
                _root = root;
                for (int idx = 0; idx < collections.Length; idx++)
                {
                    if (idx >= _quizBtns[0].transform.parent.childCount)
                        _quizBtns.Add(Instantiate(_quizBtns[0], _quizBtns[0].transform.parent));

                    var record = _quizBtns[idx];
                    record.transform.Find("Frontplate/AnimatedContent/Header/Content/Title").GetComponent<TextMeshProUGUI>().text = collections[idx].Name;
                    record.transform.Find("Frontplate/AnimatedContent/Header/Content/Description").GetComponent<TextMeshProUGUI>().text = collections[idx].Description ?? "Welcome to Quizzes";
                    if (collections[idx].Quizzes.Length > 0 && (!collections[idx].Quizzes.First().ThumbNail.IsNullOrEmpty() || !collections[idx].Quizzes.First().Image.IsNullOrEmpty()))
                    {
                        record.transform.Find("Frontplate/AnimatedContent/Image").GetComponent<Image>().sprite = !collections[idx].Quizzes.First().ThumbNail.IsNullOrEmpty() ? await IMG2Sprite.FetchImageSprite(collections[idx].Quizzes.First().ThumbNail) : await IMG2Sprite.FetchImageSprite(collections[idx].Quizzes.First().Image);
                    }
                }

                RegisterEvents();
            }

            public override void RegisterEvents()
            {
                base.RegisterEvents();

                for (int idx = 0; idx < _quizBtns.Count; idx++)
                {
                    int index = idx;
                    _quizBtns[idx].OnClicked.AddListener(() =>
                    {
                        _root.OnQuizCollectionSelect(index);
                        _onBack?.Invoke();
                        Transform.SetActive(false);
                    });
                }
            }
        }

        private IObjectResolver _container;
        private GameStore _gameStore;
        private AudioPoolManager _audioPoolManager;
        private VirtualRoomPresenter _virtualRoomPresenter;
        private IUserDataController _userDataController;
        private QuizzesHub _quizzesHub;

        [SerializeField][DebugOnly] private PressableButton _closeBtn;
        [SerializeField][DebugOnly] private PressableButton _quitBtn;

        [SerializeField][DebugOnly] private PressableButton _startBtn;
        [SerializeField][DebugOnly] private TextMeshProUGUI _titleTxt;
        [SerializeField][DebugOnly] private TextMeshProUGUI _amountTxt;

        [SerializeField][DebugOnly] private RoomStatusPerson _hostItem;
        [SerializeField][DebugOnly] private QuizzesRoomStatusPerson[] _personItems;

        [SerializeField][DebugOnly] private PressableButton _selectQuizBtn;
        [SerializeField][DebugOnly] private TextMeshProUGUI _quizNameTxt;

        [SerializeField][DebugOnly] private QuizCollectionDto[] _collections;
        [SerializeField][DebugOnly] private QuizCollectionDto _selectedCollection;

        [DebugOnly] public Transform RootContent;
        [SerializeField][DebugOnly] private SelectQuizView _selectQuizView;

        [SerializeField][DebugOnly] bool _isFirstCreate = true;
        private void OnEnable()
        {
            if (!_isFirstCreate)
                Refresh();
            _isFirstCreate = false;
        }

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
            _quizzesHub = container.Resolve<QuizzesHub>();
        }

        private void GetReferences()
        {
            _closeBtn = transform.Find("CanvasDialog/Canvas/Content/Header/Close_Btn").GetComponent<PressableButton>();
            _quitBtn = transform.Find("CanvasDialog/Canvas/Content/Header/Quit_Btn").GetComponent<PressableButton>();

            _startBtn = transform.Find("CanvasDialog/Canvas/Content/Header/Start_Btn").GetComponent<PressableButton>();
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
                return new QuizzesRoomStatusPerson
                {
                    Button = person.GetComponent<PressableButton>(),
                    NameTxt = person.Find("Frontplate/AnimatedContent/Text").GetComponent<TextMeshProUGUI>(),
                    IconImg = person.Find("Frontplate/AnimatedContent/Icon/UIButtonSpriteIcon").GetComponent<Image>(),
                };
            }).ToArray();

            _selectQuizBtn = transform.Find("CanvasDialog/Canvas/Content/Footer/SelectQuiz_Btn").GetComponent<PressableButton>();
            _quizNameTxt = transform.Find("CanvasDialog/Canvas/Content/Footer/QuizName_Txt").GetComponent<TextMeshProUGUI>();

            RootContent = transform.Find("CanvasDialog/Canvas/Content");
            _selectQuizView = new SelectQuizView(transform.Find("CanvasDialog/Canvas/QuizSelection"), _container, transform, OnBack);

            _selectQuizView.Transform.SetActive(false);
            _quizNameTxt.text = "";
        }

        private void OnBack()
        {
            RootContent.SetActive(true);
        }

        private void RegisterEvents()
        {
            _closeBtn.OnClicked.AddListener(() =>
            {
                _gameStore.HideCurrentModule(ModuleName.QuizzesRoomStatus);
            });
            _quitBtn.OnClicked.AddListener(() =>
            {
                _showPopupPublisher.Publish(new ShowPopupSignal(title: "Are you sure you want to quit the game?", yesContent: "Yes", noContent: "No", yesAction: async (value1, value2) =>
                {
                    _showLoadingPublisher.Publish(new ShowLoadingSignal());
                    await _quizzesHub.LeaveAsync();
                    _showLoadingPublisher.Publish(new ShowLoadingSignal(isShow: false));
                }, noAction: (_, _) => { }));
            });

            _startBtn.OnClicked.AddListener(async () =>
            {
                await _quizzesHub.StartGame(_selectedCollection);
                _gameStore.HideCurrentModule(ModuleName.QuizzesRoomStatus);
            });

            _selectQuizBtn.OnClicked.AddListener(async () =>
            {
                await FetchCollections();
                if (_collections.Length == 0) return;
                RootContent.SetActive(false);
                _selectQuizView.Transform.SetActive(true);
            });
        }

        public void OnQuizCollectionSelect(int index)
        {
            if (index >= _collections.Length) return;
            _selectedCollection = _collections[index];
            _quizNameTxt.text = _selectedCollection.Name;
        }

        public async Task FetchCollections()
        {
            _collections = (await _quizzesHub.GetCollections()).Collections;
            UpdateStartBtn();
            if (_collections.Length == 0)
                return;
            _ = _selectQuizView.SetupQuizzes(_collections, this);
            if (_selectedCollection == null) OnQuizCollectionSelect(0);
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

        private void UpdateStartBtn()
        {
            bool isInGame = _userDataController.ServerData.IsInGame;
            bool isHost = _userDataController.ServerData.RoomStatus.RoomStatus.Self.IsHost;
            _startBtn.SetActive(isHost && isInGame && _collections.Length > 0);
        }

        public async void Refresh()
        {
            if (!_userDataController.ServerData.IsInRoom || !_userDataController.ServerData.IsInGame)
            {
                _gameStore.GState.RemoveModel<QuizzesRoomStatusModel>();
                return;
            }

            bool isInGame = _userDataController.ServerData.IsInGame;
            var status = _userDataController.ServerData.RoomStatus.InGameStatus;

            string idPrefix = "PIN";
            _titleTxt.SetText($"{idPrefix}: {status.Id}");
            _amountTxt.SetText($"Amount: {status.Amount}");
            for (int idx = 0; idx < _personItems.Length; idx++)
                _personItems[idx].Button.SetActive(false);
            for (int idx = 0; idx < status.AllInRoom.Length; idx++)
                await UpdateCharacter(status.AllInRoom[idx].UserData);

            bool isHost = _userDataController.ServerData.RoomStatus.RoomStatus.Self.IsHost;
            UpdateStartBtn();
            _selectedCollection = _collections.Length > 0 ? _collections[0] : null;
            _selectQuizBtn.SetActive(isHost && isInGame);
        }
    }
}
