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
using System.Threading.Tasks;
using TMPro;
using UnityEditor.Search;
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
            [SerializeField][DebugOnly] protected PressableButton[] _quizBtns;
            [SerializeField][DebugOnly] protected QuizzesRoomStatusView _root;

            public SelectQuizView(Transform transform, IObjectResolver container) : base(transform, container)
            {
                _backBtn = Transform.Find("Content/Header/Back_Btn").GetComponent<PressableButton>();

                _quizBtns = new PressableButton[] {
                    Transform.Find("Content/Scroll View/Viewport/Content").GetChild(0).GetComponent<PressableButton>()
                };

                RegisterEvents();
            }

            public async UniTask SetupQuizzes(QuizCollectionDto[] collections, QuizzesRoomStatusView root)
            {
                for (int idx = 0; idx < collections.Length; idx++)
                {
                    if (idx >= _quizBtns[0].transform.parent.childCount)
                        Instantiate(_quizBtns[0], _quizBtns[0].transform.parent);

                    var record = _quizBtns[idx];
                    record.transform.Find("Frontplate/AnimatedContent/Header/Content/Title").GetComponent<TextMeshProUGUI>().text = collections[idx].Name;
                    record.transform.Find("Frontplate/AnimatedContent/Header/Content/Description").GetComponent<TextMeshProUGUI>().text = collections[idx].Description ?? "Welcome to Quizzes";
                    if (collections[idx].Quizzes.Length > 0 && (!collections[idx].Quizzes.First().ThumbNail.IsNullOrEmpty() || !collections[idx].Quizzes.First().Image.IsNullOrEmpty()))
                    {
                        record.transform.Find("Frontplate/AnimatedContent/Image").GetComponent<Image>().sprite = !collections[idx].Quizzes.First().ThumbNail.IsNullOrEmpty() ? await IMG2Sprite.FetchImageSprite(collections[idx].Quizzes.First().ThumbNail) : await IMG2Sprite.FetchImageSprite(collections[idx].Quizzes.First().Image);
                    }
                }
            }

            public override void RegisterEvents()
            {
                base.RegisterEvents();

                for (int idx = 0; idx < _quizBtns.Length; idx++)
                {
                    _quizBtns[idx].OnClicked.AddListener(() =>
                    {
                        _root.OnQuizSelect(idx);
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

        [SerializeField][DebugOnly] private QuizzesRoomStatusPerson[] _personItems;

        [SerializeField][DebugOnly] private PressableButton _selectQuizBtn;
        [SerializeField][DebugOnly] private TextMeshProUGUI _quizNameTxt;

        [SerializeField][DebugOnly] private QuizCollectionDto[] _collections;
        [SerializeField][DebugOnly] private QuizCollectionDto _selectedCollection;

        [SerializeField][DebugOnly] private SelectQuizView _selectQuizView;

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
            _closeBtn = transform.Find("CanvasDialog/Canvas/Header/RightSide/Close_Btn").GetComponent<PressableButton>();
            _quitBtn = transform.Find("CanvasDialog/Canvas/Header/Quit_Btn").GetComponent<PressableButton>();

            _startBtn = transform.Find("CanvasDialog/Canvas/Header/Start_Btn").GetComponent<PressableButton>();
            _titleTxt = transform.Find("CanvasDialog/Canvas/Header/Content/Title").GetComponent<TextMeshProUGUI>();
            _amountTxt = transform.Find("CanvasDialog/Canvas/Header/Content/Amount").GetComponent<TextMeshProUGUI>();

            var list = transform.Find("CanvasDialog/Canvas/Content/Scroll View/Viewport/Content");
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

            _selectQuizBtn = transform.Find("CanvasDialog/Canvas/Footer/SelectQuiz_Btn").GetComponent<PressableButton>();
            _quizNameTxt = transform.Find("CanvasDialog/Canvas/Footer/SelectQuiz_Btn").GetComponent<TextMeshProUGUI>();

            _selectQuizView = new SelectQuizView(transform.Find("CanvasDialog/Canvas/ToolSelection"), _container);

            _selectQuizView.Transform.SetActive(false);
            _quizNameTxt.text = "";
        }

        private void RegisterEvents()
        {
            _quitBtn.OnClicked.AddListener(() =>
            {
                _showPopupPublisher.Publish(new ShowPopupSignal(title: "Are you sure you want to quit the game?", yesContent: "Yes", noContent: "No", yesAction: async (value1, value2) =>
                {
                    _showLoadingPublisher.Publish(new ShowLoadingSignal());
                    await _quizzesHub.LeaveAsync();

                    _gameStore.GState.RemoveModel<QuizzesRoomStatusModel>();
                    await _gameStore.GetOrCreateModule<LandingScreen, LandingScreenModel>(
                        "", ViewName.Unity, ModuleName.LandingScreen);

                    _showLoadingPublisher.Publish(new ShowLoadingSignal(isShow: false));
                }, noAction: (_, _) => { }));
            });

            _startBtn.OnClicked.AddListener(async () =>
            {
                await _virtualRoomPresenter.StartGame(_selectedCollection);
                _gameStore.GState.RemoveModel<QuizzesRoomStatusModel>();
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
        }

        public void OnQuizSelect(int index)
        {
            _quizNameTxt.text = _collections[index].Name;
        }

        public async Task FetchCollections()
        {
            _collections = (await _quizzesHub.GetCollections()).Collections;
            _ = _selectQuizView.SetupQuizzes(_collections, this);
        }

        public override async void OnReady()
        {
            GetReferences();
            RegisterEvents();

            await FetchCollections();

            Refresh();
        }

        public void Refresh()
        {
            if (!_userDataController.ServerData.IsInRoom)
            {
                _gameStore.GState.RemoveModel<QuizzesRoomStatusModel>();
                return;
            }

            bool isInGame = _userDataController.ServerData.IsInGame;
            var status = _userDataController.ServerData.RoomStatus.InGameStatus;

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

                _personItems[idx].NameTxt.text = status.Others[idx].UserData.Name;
            }
            bool isHost = _userDataController.ServerData.RoomStatus.RoomStatus.Self.IsHost;
            _startBtn.SetActive(isHost && isInGame && _collections.Length > 0);
            _selectedCollection = _collections.Length > 0 ? _collections[0] : null;
            _selectQuizBtn.SetActive(isHost && isInGame);
        }
    }
}
