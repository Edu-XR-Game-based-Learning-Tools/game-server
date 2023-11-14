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

    public interface IQuizzesManualHandling
    {
        void OnStart();

        void OnDonePreview();

        void OnEndQuestion();

        void OnNextQuestion();

        void OnEndQuiz();

        void OnEndSession();
    }

    [System.Serializable]
    public class ObjectQuizVisual : SubView
    {
        [SerializeField][DebugOnly] protected Image _imageObject2D;
        [SerializeField][DebugOnly] protected Transform _rtObject3D;
        [SerializeField][DebugOnly] protected PressableButton _imageToggleBtn;
        [SerializeField][DebugOnly] protected PressableButton _3dToggleBtn;
        [SerializeField][DebugOnly] protected bool _isImageToggle = true;

        public ObjectQuizVisual(Transform transform, IObjectResolver container, Transform viewRoot, System.Action onBack = null, string prefix = "Content/Layout") : base(transform, container, viewRoot, onBack)
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

    public class QuizzesQuestionView : UnityView, IQuizzesManualHandling
    {
        [System.Serializable]
        public class HeaderView : SubView
        {
            private readonly QuizzesHub _quizzesHub;
            private readonly IUserDataController _userDataController;

            [SerializeField][DebugOnly] private TextMeshProUGUI _noQuestionTxt;
            [SerializeField][DebugOnly] private PressableButton _nextBtn;
            [SerializeField][DebugOnly] private QuizzesQuestionView _rootView;

            public HeaderView(Transform transform, IObjectResolver container, Transform viewRoot, System.Action onBack = null) : base(transform, container, viewRoot, onBack)
            {
                _quizzesHub = container.Resolve<QuizzesHub>();
                _userDataController = container.Resolve<IUserDataController>();

                _noQuestionTxt = transform.Find("NoQuestion_Txt").GetComponent<TextMeshProUGUI>();
                _nextBtn = transform.Find("Next_Btn").GetComponent<PressableButton>();

                _rootView = transform.GetComponent<QuizzesQuestionView>();

                RegisterEvents();
            }

            [SerializeField][DebugOnly] private bool _isShowScoreboard = false;
            public override void RegisterEvents()
            {
                _nextBtn.OnClicked.AddListener(async () =>
                {
                    var status = _userDataController.ServerData.RoomStatus.InGameStatus;
                    if (_isShowScoreboard)
                    {
                        _isShowScoreboard = true;
                        await _rootView.ShowScoreboard();
                        _isShowScoreboard = false;
                        return;
                    }

                    if (status.JoinQuizzesData.CurrentQuestionIdx == status.QuizCollection.Quizzes.Length)
                        _ = _quizzesHub.EndSession();
                    else
                        _ = _quizzesHub.NextQuestion();
                });
            }

            public void UpdateContent(bool isShowNextBtn = false)
            {
                var status = _userDataController.ServerData.RoomStatus.InGameStatus;
                if (status == null) return;
                _noQuestionTxt.text = $"{Mathf.Clamp(status.JoinQuizzesData.CurrentQuestionIdx + 1, 0, status.QuizCollection.Quizzes.Length)}/{status.QuizCollection.Quizzes.Length}";

                _nextBtn.SetActive(isShowNextBtn);
                if (!isShowNextBtn) return;
                _nextBtn.transform.Find("Frontplate/AnimatedContent/Text").GetComponent<TextMeshProUGUI>().text = status.JoinQuizzesData.CurrentQuestionIdx < status.QuizCollection.Quizzes.Length ? "Next" : "Finish";
            }
        }

        [System.Serializable]
        public class PreviewView : ObjectQuizVisual
        {
            private readonly QuizzesHub _quizzesHub;

            [SerializeField][DebugOnly] private TextMeshProUGUI _questionTxt;
            [SerializeField][DebugOnly] private Microsoft.MixedReality.Toolkit.UX.Slider _progressSlider;

            [SerializeField] private float _previewDuration = 4f;
            [SerializeField][DebugOnly] private QuizzesQuestionView _rootView;

            public PreviewView(Transform transform, IObjectResolver container, Transform viewRoot, System.Action onBack = null) : base(transform, container, viewRoot, onBack)
            {
                _quizzesHub = container.Resolve<QuizzesHub>();

                _questionTxt = transform.Find("Content/Question_Txt").GetComponent<TextMeshProUGUI>();
                _progressSlider = transform.Find("Footer/Slider").GetComponent<Microsoft.MixedReality.Toolkit.UX.Slider>();

                _rootView = transform.GetComponent<QuizzesQuestionView>();

                Init();
            }

            public void Init()
            {
                _questionTxt.text = "Question?";
                _progressSlider.Value = 0;
            }

            public async UniTask UpdateContent(QuizDto data)
            {
                _questionTxt.text = data.Question;
                _imageObject2D.sprite = await IMG2Sprite.FetchImageSprite(data.Image);
                _progressSlider.Value = 0;

                DOTween.To(() => _progressSlider.Value, (value) => _progressSlider.Value = value, 1f, _previewDuration).onComplete = () =>
                {
                    _ = _quizzesHub.DonePreview();
                };
            }
        }

        [System.Serializable]
        public class QuestionView : ObjectQuizVisual
        {
            private readonly QuizzesHub _quizzesHub;
            private readonly IUserDataController _userDataController;

            [DebugOnly] public Transform _countdownTransform;
            [DebugOnly] public Transform _resultTransform;

            [SerializeField][DebugOnly] private TextMeshProUGUI _countdownTxt;
            [SerializeField][DebugOnly] private TextMeshProUGUI _noAnswerTxt;
            [SerializeField][DebugOnly] private TextMeshProUGUI _questionTxt;

            [SerializeField][DebugOnly] private Transform[] _resultOptionTransforms;

            [SerializeField][DebugOnly] private Transform[] _optionTransforms;

            [SerializeField] private float _previewDuration = 4f;
            [SerializeField][DebugOnly] private QuizzesQuestionView _rootView;

            public QuestionView(Transform transform, IObjectResolver container, Transform viewRoot, System.Action onBack = null) : base(transform, container, viewRoot, onBack, "Countdown/Content/Layout")
            {
                _quizzesHub = container.Resolve<QuizzesHub>();
                _userDataController = container.Resolve<IUserDataController>();

                _countdownTransform = transform.Find("Countdown");
                _resultTransform = transform.Find("Result");

                _countdownTxt = transform.Find("Countdown/Header/Countdown_Txt").GetComponent<TextMeshProUGUI>();
                _noAnswerTxt = transform.Find("Countdown/Header/Layout/NoAnswer_Txt").GetComponent<TextMeshProUGUI>();
                _questionTxt = transform.Find("Countdown/Content/Question_Txt").GetComponent<TextMeshProUGUI>();

                var optionParent = transform.Find("Result/Layout");
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

                _rootView = transform.GetComponent<QuizzesQuestionView>();

                Init();
            }

            private void EnableResultOption(bool[] options = null, int[] answerAmounts = null)
            {
                bool isNullOption = options == null;
                options ??= Enumerable.Repeat(false, _resultOptionTransforms.Length).ToArray();
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

            public void Init()
            {
                _questionTxt.text = "Question?";
                _noAnswerTxt.text = "Answer: 0";
                EnableResultOption();
                EnableOption();
            }

            private void StartCountdown(int duration)
            {
                _countdownTxt.text = $"{duration}s";
                DOTween.To(() => int.Parse(_countdownTxt.text[..^1]), (value) => _countdownTxt.text = $"{value}s", 0, duration + _previewDuration).onComplete = () =>
                {
                    _ = _quizzesHub.EndQuestion();
                };
            }

            public async UniTask UpdateContent(bool isShowResult = false)
            {
                QuizzesStatusResponse status = _userDataController.ServerData.RoomStatus.InGameStatus;
                QuizDto data = status.QuizCollection.Quizzes[status.JoinQuizzesData.CurrentQuestionIdx];
                _countdownTransform.SetActive(!isShowResult);
                _resultTransform.SetActive(isShowResult);

                if (isShowResult)
                {
                    bool[] options = Enumerable.Repeat(false, _optionTransforms.Length).ToArray();
                    int[] answerAmounts = Enumerable.Repeat(true, _optionTransforms.Length).Select((_, idx) => status.Students.Count(ele => ele.AnswerIdx == idx)).ToArray();
                    options[data.CorrectIdx] = true;
                    EnableResultOption(options, answerAmounts);
                    EnableOption(options);
                    return;
                }

                _questionTxt.text = data.Question;
                EnableOption();
                for (int idx = 0; idx < _optionTransforms.Length; idx++)
                {
                    bool isActive = idx < data.Answers.Length && !data.Answers[idx].IsNullOrEmpty();
                    _resultOptionTransforms[idx].SetActive(isActive);
                    _optionTransforms[idx].SetActive(isActive);
                    if (idx >= data.Answers.Length) continue;
                    _optionTransforms[idx].Find("Frontplate/AnimatedContent/Text").GetComponent<TextMeshProUGUI>().text = data.Answers[idx];
                }
                _imageObject2D.sprite = await IMG2Sprite.FetchImageSprite(data.Image);

                StartCountdown(data.Duration);
            }

            public void IncreaseNoAnswer()
            {
                _noAnswerTxt.text = $"Answer: {int.Parse(_noAnswerTxt.text.Substring("Answer: ".Length, _noAnswerTxt.text.Length)) + 1}";
            }
        }

        [System.Serializable]
        public class ScoreboardView : SubView
        {
            [SerializeField][DebugOnly] protected Transform _scrollContent;
            protected UserDataController _userDataController;

            public ScoreboardView(Transform transform, IObjectResolver container, Transform viewRoot, System.Action onBack = null, string scrollPrefix = "Content") : base(transform, container, viewRoot, onBack)
            {
                _userDataController = container.Resolve<IUserDataController>() as UserDataController;
                _scrollContent = transform.Find($"{scrollPrefix}/Scroll View/Viewport/Content");
            }

            public virtual async UniTask UpdateContent(QuizzesUserData[] quizPlayers = null)
            {
                RoomStatusVM roomStatus = _userDataController.ServerData.RoomStatus;
                quizPlayers ??= roomStatus.InGameStatus.Students.OrderByDescending(ele =>
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

            public LeaderBoardView(Transform transform, IObjectResolver container, Transform viewRoot, System.Action onBack = null) : base(transform, container, viewRoot, onBack, "Footer")
            {
                _top3[0] = transform.Find("Content/Layout").GetChild(1);
                _top3[1] = transform.Find("Content/Layout").GetChild(2);
                _top3[2] = transform.Find("Content/Layout").GetChild(0);
            }

            public override async UniTask UpdateContent(QuizzesUserData[] _ = null)
            {
                RoomStatusVM roomStatus = _userDataController.ServerData.RoomStatus;
                if (roomStatus.InGameStatus == null) return;
                var quizPlayers = roomStatus.InGameStatus.Students.OrderByDescending(ele =>
                     ele.Score).ToArray();
                for (int idx = 0; idx < _top3.Length; idx++)
                {
                    _top3[idx].SetActive(idx < quizPlayers.Length);
                    if (idx >= quizPlayers.Length)
                        continue;

                    _top3[idx].Find("Avatar").GetComponent<Image>().sprite = await _userDataController.LocalUserCache.GetSprite(quizPlayers[idx].UserData.AvatarPath);
                    _top3[idx].Find("Name_Txt").GetComponent<TextMeshProUGUI>().text = quizPlayers[idx].UserData.Name;
                    _top3[idx].Find("Score_Txt").GetComponent<TextMeshProUGUI>().text = $"{quizPlayers[idx].Score}";
                    _top3[idx].Find("Rank_Txt").GetComponent<TextMeshProUGUI>().text = $"{quizPlayers[idx].Rank}";
                }

                await base.UpdateContent(quizPlayers.Skip(3).ToArray());
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

        public void Init(IObjectResolver container)
        {
            _container = container;
            _gameStore = container.Resolve<GameStore>();
            _audioPoolManager = (AudioPoolManager)container.Resolve<IReadOnlyList<IPoolManager>>().ElementAt((int)PoolName.Audio);
            _virtualRoomPresenter = container.Resolve<VirtualRoomPresenter>();
            _userDataController = container.Resolve<IUserDataController>();
            _quizzesHub = container.Resolve<QuizzesHub>();

            OnReady();
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

        private async UniTask SetupQuestion()
        {
            QuizzesStatusResponse status = _userDataController.ServerData.RoomStatus.InGameStatus;
            QuizDto quiz = status.QuizCollection.Quizzes[status.JoinQuizzesData.CurrentQuestionIdx];
            _headerView.UpdateContent();
            _ = _previewView.UpdateContent(quiz);
            _ = _questionView.UpdateContent();
            _object3DContainer.Find("Mesh").GetComponent<MeshFilter>().sharedMesh = await MeshFromURL.FetchModel(quiz.Model);
        }

        public async UniTask ShowScoreboard()
        {
            await _scoreboardView.UpdateContent();

            _questionView.Transform.SetActive(false);
            _scoreboardView.Transform.SetActive(true);
            _headerView.UpdateContent(true);
        }

        #region Handle Hub Response

        public async void OnStart()
        {
            await SetupQuestion();

            transform.SetActive(true);
            _previewView.Transform.SetActive(true);
            _questionView.Transform.SetActive(false);
            _scoreboardView.Transform.SetActive(false);
            _leaderBoardView.Transform.SetActive(false);
        }

        public void OnDonePreview()
        {
            _previewView.Transform.SetActive(false);
            _questionView.Transform.SetActive(true);
        }

        public void OnEndQuestion()
        {
            _headerView.UpdateContent(true);
            _ = _questionView.UpdateContent(true);
        }

        public async void OnNextQuestion()
        {
            await SetupQuestion();

            _questionView.Transform.SetActive(false);
            _scoreboardView.Transform.SetActive(false);
            _previewView.Transform.SetActive(true);
        }

        public void OnEndQuiz()
        {
            _ = _leaderBoardView.UpdateContent();
            _headerView.UpdateContent(true);

            _questionView.Transform.SetActive(false);
            _scoreboardView.Transform.SetActive(false);
            _leaderBoardView.Transform.SetActive(true);
        }

        public void OnEndSession()
        {
            transform.SetActive(false);
        }

        public void OnAnswer(AnswerData _)
        {
            _questionView.IncreaseNoAnswer();
        }

        #endregion Handle Hub Response

        public void Refresh()
        {
        }
    }
}
