using Core.Business;
using Core.Extension;
using Core.Framework;
using Core.Module;
using Core.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Microsoft.MixedReality.Toolkit.UX;
using Shared.Extension;
using Shared.Network;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using static Core.View.QuizzesAnswerView;

namespace Core.View
{
    [System.Serializable]
    public class ObjectQuizVisual : SubView
    {
        [SerializeField][DebugOnly] protected Image _imageObject2D;
        [SerializeField][DebugOnly] protected Transform _rtObject3D;
        [SerializeField][DebugOnly] protected PressableButton _imageToggleBtn;
        [SerializeField][DebugOnly] protected PressableButton _3dToggleBtn;
        [SerializeField][DebugOnly] protected bool _isImageToggle = true;

        public ObjectQuizVisual(Transform transform, IObjectResolver container, Transform viewRoot, System.Action onBack = null, string prefix = "Content/Layout/") : base(transform, container, viewRoot, onBack)
        {
            _imageObject2D = transform.Find($"{prefix}/Visual/Image").GetComponent<Image>();
            _rtObject3D = transform.Find($"{prefix}/Visual/RT_3D");
            _imageToggleBtn = transform.Find($"{prefix}/Visual/PreviewToggle/Image_Btn").GetComponent<PressableButton>();
            _3dToggleBtn = transform.Find($"{prefix}/Visual/PreviewToggle/3D_Btn").GetComponent<PressableButton>();

            Init();
            RegisterEvents();
        }

        private void ToggleVisual(bool isImageToggle = true)
        {
            _isImageToggle = isImageToggle;
            _imageObject2D.SetActive(_isImageToggle);
            _rtObject3D.SetActive(!_isImageToggle);
            Color color = _imageToggleBtn.transform.Find("Backplate").GetComponent<Image>().color;
            _imageToggleBtn.transform.Find("Backplate").GetComponent<Image>().color = new Color(color.r, color.g, color.b, _isImageToggle ? 1f : 0);
            _3dToggleBtn.transform.Find("Backplate").GetComponent<Image>().color = new Color(color.r, color.g, color.b, !_isImageToggle ? 1f : 0);
        }

        private void Init()
        {
            ToggleVisual();
        }

        public override void RegisterEvents()
        {
            _imageToggleBtn.OnClicked.AddListener(() =>
            {
                ToggleVisual();
            });
            _3dToggleBtn.OnClicked.AddListener(() =>
            {
                ToggleVisual(false);
            });
        }
    }

    public class QuizzesQuestionView : UnityView
    {
        [System.Serializable]
        public class HeaderView : SubView
        {
            [SerializeField][DebugOnly] private TextMeshProUGUI _noQuestionTxt;
            [SerializeField][DebugOnly] private PressableButton _nextBtn;
            [SerializeField][DebugOnly] private QuizzesQuestionView _rootView;

            public HeaderView(Transform transform, IObjectResolver container, Transform viewRoot, System.Action onBack = null) : base(transform, container, viewRoot, onBack)
            {
                _noQuestionTxt = transform.Find("Header/NoQuestion_Txt").GetComponent<TextMeshProUGUI>();
                _nextBtn = transform.Find("Header/Next_Btn").GetComponent<PressableButton>();

                RegisterEvents();
            }

            public HeaderView Init(QuizzesQuestionView rootView)
            {
                _rootView = rootView;

                return this;
            }

            public override void RegisterEvents()
            {
                _nextBtn.OnClicked.AddListener(() =>
                {
                    _rootView.NextQuestion();
                });
            }

            public void UpdateContent(string noQuestion)
            {
                _noQuestionTxt.text = noQuestion;
            }
        }

        [System.Serializable]
        public class PreviewView : ObjectQuizVisual
        {
            [SerializeField][DebugOnly] private TextMeshProUGUI _questionTxt;
            [SerializeField][DebugOnly] private Microsoft.MixedReality.Toolkit.UX.Slider _progressSlider;

            [SerializeField] private float _previewDuration = 10f;
            [SerializeField][DebugOnly] private QuizzesQuestionView _rootView;

            public PreviewView(Transform transform, IObjectResolver container, Transform viewRoot, System.Action onBack = null) : base(transform, container, viewRoot, onBack)
            {
                _questionTxt = transform.Find("Content/Question_Txt").GetComponent<TextMeshProUGUI>();
                _progressSlider = transform.Find("Footer/Slider").GetComponent<Microsoft.MixedReality.Toolkit.UX.Slider>();
            }

            public PreviewView Init(QuizzesQuestionView rootView)
            {
                _rootView = rootView;
                _questionTxt.text = "Question?";
                _progressSlider.Value = 0;

                return this;
            }

            public async UniTask UpdateContent(QuizDto data)
            {
                _questionTxt.text = data.Question;
                _imageObject2D.sprite = await IMG2Sprite.FetchImageSprite(data.Image);

                DOTween.To(() => _progressSlider.Value, (value) => _progressSlider.Value = value, 1f, _previewDuration).onComplete = () =>
                {
                    _rootView.DonePreview();
                };
            }
        }

        [System.Serializable]
        public class QuestionView : ObjectQuizVisual
        {
            [DebugOnly] public Transform countdownTransform;
            [DebugOnly] public Transform resultTransform;

            [SerializeField][DebugOnly] private TextMeshProUGUI _countdownTxt;
            [SerializeField][DebugOnly] private TextMeshProUGUI _noAnswerTxt;
            [SerializeField][DebugOnly] private TextMeshProUGUI _questionTxt;

            [SerializeField][DebugOnly] private Transform[] _resultOptionTransforms;

            [SerializeField][DebugOnly] private Transform[] _optionTransforms;

            [SerializeField][DebugOnly] private QuizzesQuestionView _rootView;

            public QuestionView(Transform transform, IObjectResolver container, Transform viewRoot, System.Action onBack = null) : base(transform, container, viewRoot, onBack, "Countdown/Content/Layout/")
            {
                countdownTransform = transform.Find("Countdown");
                resultTransform = transform.Find("Result");

                _countdownTxt = transform.Find("Countdown/Header/Countdown_Txt").GetComponent<TextMeshProUGUI>();
                _noAnswerTxt = transform.Find("Countdown/Header/Layout/NoAnswer_Txt").GetComponent<TextMeshProUGUI>();
                _questionTxt = transform.Find("Countdown/Content/Question_Txt").GetComponent<TextMeshProUGUI>();

                var optionParent = transform.Find("Result/Options");
                _resultOptionTransforms = new Transform[optionParent.childCount];
                for (int idx = 0; idx < optionParent.childCount; idx++)
                {
                    _resultOptionTransforms[idx] = optionParent.GetChild(idx);
                }

                optionParent = transform.Find("Footer/Options");
                _optionTransforms = new Transform[optionParent.childCount];
                for (int idx = 0; idx < optionParent.childCount; idx++)
                {
                    _optionTransforms[idx] = optionParent.GetChild(idx);
                }
            }

            private void EnableResultOption(bool[] options = null, int[] answerAmounts = null)
            {
                bool isNullOption = options == null;
                options ??= Enumerable.Repeat(true, _resultOptionTransforms.Length).ToArray();
                answerAmounts ??= Enumerable.Repeat(0, _resultOptionTransforms.Length).ToArray();

                for (int idx = 0; idx < _resultOptionTransforms.Length; idx++)
                {
                    _resultOptionTransforms[idx].transform.Find("Frontplate/AnimatedContent/Icon").SetActive(!isNullOption && idx < options.Length && options[idx]);
                    _resultOptionTransforms[idx].transform.Find("Frontplate/AnimatedContent/Text").GetComponent<TextMeshProUGUI>().text = $"{(idx < answerAmounts.Length ? answerAmounts[idx] : 0)}";
                }
            }

            private void EnableOption(bool[] options = null)
            {
                bool isNullOption = options == null;
                options ??= Enumerable.Repeat(true, _optionTransforms.Length).ToArray();

                for (int idx = 0; idx < _optionTransforms.Length; idx++)
                {
                    Color color = (idx >= options.Length || options[idx]) ? new Color(1f, 1f, 1f) : new Color(70f / 255, 70f / 255, 70f / 255);
                    _optionTransforms[idx].transform.Find("Backplate").GetComponent<Image>().color = color;
                    _optionTransforms[idx].transform.Find("Frontplate/AnimatedContent/Icon").SetActive(!isNullOption && idx < options.Length && options[idx]);
                }
            }

            public QuestionView Init(QuizzesQuestionView rootView)
            {
                _rootView = rootView;
                _questionTxt.text = "Question?";
                EnableResultOption();
                EnableOption();

                return this;
            }

            private void StartCountdown(int duration)
            {
                _countdownTxt.text = $"{duration}";
                DOTween.To(() => int.Parse(_countdownTxt.text), (value) => _countdownTxt.text = $"{value}", 0, duration).onComplete = () =>
                {
                    _rootView.EndQuestion();
                };
            }

            public async UniTask UpdateContent(QuizDto data, int answerAmount = 0)
            {
                _questionTxt.text = data.Question;
                _noAnswerTxt.text = $"Answer: {answerAmount}";
                for (int idx = 0; idx < _optionTransforms.Length; idx++)
                {
                    _optionTransforms[idx].SetActive(idx < data.Answers.Length);
                    if (idx >= data.Answers.Length) continue;
                    _optionTransforms[idx].Find("Frontplate/AnimatedContent/Text").GetComponent<TextMeshProUGUI>().text = data.Answers[idx];
                }
                _imageObject2D.sprite = await IMG2Sprite.FetchImageSprite(data.Image);

                StartCountdown(data.Duration);
            }

            public void IncreaseNoAnswer()
            {
                _noAnswerTxt.text = $"Answer: {int.Parse(_noAnswerTxt.text) + 1}";
            }
        }

        [System.Serializable]
        public class ScoreboardView : SubView
        {
            [SerializeField][DebugOnly] protected Transform _scrollContent;
            protected UserDataController _userDataController;

            public ScoreboardView(Transform transform, IObjectResolver container, Transform viewRoot, System.Action onBack = null, string scrollPrefix = "Content/") : base(transform, container, viewRoot, onBack)
            {
                _userDataController = container.Resolve<IUserDataController>() as UserDataController;
                _scrollContent = transform.Find($"{scrollPrefix}/Scroll View/Viewport/Content");
            }

            public async UniTask UpdateData(RoomStatusVM roomStatus, QuizzesUserData[] quizPlayers = null)
            {
                quizPlayers ??= roomStatus.InGameStatus.AllInRoom.OrderByDescending(ele =>
                     ele.Score).ToArray();
                if (quizPlayers.Length < _scrollContent.childCount)
                    for (int idx = 0; idx < _scrollContent.childCount; idx++)
                    {
                        if (idx >= quizPlayers.Length)
                            _scrollContent.GetChild(idx).SetActive(false);
                    }

                for (int idx = 0; idx < quizPlayers.Length; idx++)
                {
                    if (idx >= _scrollContent.childCount)
                        Instantiate(_scrollContent.GetChild(0), _scrollContent);

                    var record = _scrollContent.GetChild(idx);
                    record.Find("Avatar").GetComponent<Image>().sprite = await _userDataController.LocalUserCache.GetSprite(quizPlayers[idx].UserData.AvatarPath);
                    record.Find("Name_Txt").GetComponent<TextMeshProUGUI>().text = quizPlayers[idx].UserData.Name;
                    record.Find("Score_Txt").GetComponent<TextMeshProUGUI>().text = $"{quizPlayers[idx].Score}";
                }
            }
        }

        [System.Serializable]
        public class LeaderBoardView : ScoreboardView
        {
            [SerializeField][DebugOnly] protected Transform[] _top3 = new Transform[3];

            public LeaderBoardView(Transform transform, IObjectResolver container, Transform viewRoot, System.Action onBack = null) : base(transform, container, viewRoot, onBack, "Footer/")
            {
                _top3[0] = transform.Find("Content/Layout").GetChild(1);
                _top3[1] = transform.Find("Content/Layout").GetChild(2);
                _top3[2] = transform.Find("Content/Layout").GetChild(0);
            }

            public async UniTask UpdateData(RoomStatusVM roomStatus)
            {
                var quizPlayers = roomStatus.InGameStatus.AllInRoom.OrderByDescending(ele =>
                     ele.Score).ToArray();
                for (int idx = 0; idx < _top3.Length; idx++)
                {
                    _top3[idx].Find("Avatar").GetComponent<Image>().sprite = await _userDataController.LocalUserCache.GetSprite(quizPlayers[idx].UserData.AvatarPath);
                    _top3[idx].Find("Name_Txt").GetComponent<TextMeshProUGUI>().text = quizPlayers[idx].UserData.Name;
                    _top3[idx].Find("Score_Txt").GetComponent<TextMeshProUGUI>().text = $"{quizPlayers[idx].Score}";
                    _top3[idx].Find("Rank_Txt").GetComponent<TextMeshProUGUI>().text = $"{quizPlayers[idx].Rank}";
                }

                await UpdateData(roomStatus, quizPlayers.Skip(3).ToArray());
            }
        }

        private IObjectResolver _container;
        private GameStore _gameStore;
        private AudioPoolManager _audioPoolManager;
        private VirtualRoomPresenter _virtualRoomPresenter;
        private IUserDataController _userDataController;
        private QuizzesHub _quizzesHub;

        [SerializeField][DebugOnly] private Transform _object3DContainer;

        [SerializeField][DebugOnly] private HeaderView _headerView;
        [SerializeField][DebugOnly] private PreviewView _previewView;
        [SerializeField][DebugOnly] private QuestionView _questionView;
        [SerializeField][DebugOnly] private ScoreboardView _scoreboardView;
        [SerializeField][DebugOnly] private LeaderBoardView _leaderBoardView;

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

            transform.SetActive(false);
        }

        private void GetReferences()
        {
            _object3DContainer = transform.Find("3D_Renderer/Object");

            _headerView = new(transform.Find("Canvas/Header"), _container, transform);
            _previewView = new(transform.Find("Canvas/Preview"), _container, transform);
            _questionView = new(transform.Find("Canvas/Question"), _container, transform);
            _scoreboardView = new(transform.Find("Canvas/Scoreboard"), _container, transform);
            _leaderBoardView = new(transform.Find("Canvas/LeaderBoard"), _container, transform);
        }

        public override void OnReady()
        {
            GetReferences();

            Refresh();
        }

        public async UniTask SetupQuestion(QuizCollectionDto collection, int questionIdx = 0)
        {
            _headerView.UpdateContent($"{questionIdx + 1}/{collection.Quizzes.Length}");
            _ = _previewView.UpdateContent(collection.Quizzes[questionIdx]);
            _ = _questionView.UpdateContent(collection.Quizzes[questionIdx]);
            _object3DContainer.Find("Object").GetComponent<MeshFilter>().sharedMesh = await MeshFromURL.FetchModel(collection.Quizzes[questionIdx].Model);
        }

        public async UniTask StartGame(QuizCollectionDto collection)
        {
            transform.SetActive(true);
            await _quizzesHub.StartGame(collection);
            _previewView.Transform.SetActive(true);
            _questionView.Transform.SetActive(false);
            _scoreboardView.Transform.SetActive(false);
            _leaderBoardView.Transform.SetActive(false);

            _ = SetupQuestion(collection);
        }

        public async void DonePreview()
        {
            await _quizzesHub.DonePreview();
            _previewView.Transform.SetActive(false);
            _questionView.Transform.SetActive(true);
        }

        public async void EndQuestion()
        {
            await _quizzesHub.EndQuestion();
            _questionView.Transform.SetActive(false);
            _scoreboardView.Transform.SetActive(true);
        }

        public async void NextQuestion()
        {
            QuizzesStatusResponse status = _userDataController.ServerData.RoomStatus.InGameStatus;
            if (status.JoinQuizzesData.CurrentQuestionIdx < status.QuizCollection.Quizzes.Length - 1)
            {
                await _quizzesHub.NextQuestion();
                await SetupQuestion(status.QuizCollection, (int)status.JoinQuizzesData.CurrentQuestionIdx);
                _scoreboardView.Transform.SetActive(false);
                _previewView.Transform.SetActive(true);
            }
            else
            {
                await _quizzesHub.EndQuestion();
                _scoreboardView.Transform.SetActive(false);
                _leaderBoardView.Transform.SetActive(true);
            }
        }

        public void OnAnswer(AnswerData _)
        {
            _questionView.IncreaseNoAnswer();
        }

        public void Refresh()
        {
        }
    }
}
