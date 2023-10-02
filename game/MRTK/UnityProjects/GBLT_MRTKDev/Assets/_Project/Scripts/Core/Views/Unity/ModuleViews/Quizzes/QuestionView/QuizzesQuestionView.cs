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
        [SerializeField][DebugOnly] private Image _imageObject2D;
        [SerializeField][DebugOnly] private Transform _rtObject3D;
        [SerializeField][DebugOnly] private PressableButton _imageToggleBtn;
        [SerializeField][DebugOnly] private PressableButton _3dToggleBtn;
        [SerializeField][DebugOnly] private bool _isImageToggle = true;

        public ObjectQuizVisual(Transform transform, string prefix = "Content/Layout/") : base(transform)
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

            public HeaderView(Transform transform) : base(transform)
            {
                _noQuestionTxt = transform.Find("Header/NoQuestion_Txt").GetComponent<TextMeshProUGUI>();
                _nextBtn = transform.Find("Header/Next_Btn").GetComponent<PressableButton>();

                RegisterEvents();
            }

            public override void RegisterEvents()
            {
                _nextBtn.OnClicked.AddListener(() =>
                {
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

            [SerializeField] private float _previewDuration;
            [SerializeField][DebugOnly] private QuizzesQuestionView _rootView;

            public PreviewView(Transform transform) : base(transform)
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

            public void UpdateContent(string question)
            {
                _questionTxt.text = question;
                DOTween.To(() => _progressSlider.Value, (value) => _progressSlider.Value = value, 1f, _previewDuration).onComplete = () =>
                {
                    _rootView.OnDoneShowPreview();
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

            public QuestionView(Transform transform) : base(transform, "Countdown/Content/Layout/")
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

            public void StartCountdown(int duration)
            {
                _countdownTxt.text = $"{duration}";
                DOTween.To(() => int.Parse(_countdownTxt.text), (value) => _countdownTxt.text = $"{value}", 0, duration).onComplete = () =>
                {
                    _rootView.OnEndQuestion();
                };
            }

            public void UpdateContent(string question, int answerAmount)
            {
                _questionTxt.text = question;
                _noAnswerTxt.text = $"{answerAmount}";
            }
        }

        [System.Serializable]
        public class ScoreboardView : SubView
        {
            [SerializeField][DebugOnly] protected Transform _scrollContent;

            public ScoreboardView(Transform transform, string scrollPrefix = "Content/") : base(transform)
            {
                _scrollContent = transform.Find($"{scrollPrefix}/Scroll View/Viewport/Content");
            }

            public async UniTask UpdateData(RoomStatusVM roomStatus, PublicUserData[] quizPlayers = null)
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
                    record.Find("Avatar").GetComponent<Image>().sprite = await roomStatus.LocalUserCache.GetUserAvatar(quizPlayers[idx].AvatarPath);
                    record.Find("Name_Txt").GetComponent<TextMeshProUGUI>().text = quizPlayers[idx].Name;
                    record.Find("Score_Txt").GetComponent<TextMeshProUGUI>().text = $"{quizPlayers[idx].Score}";
                }
            }
        }

        [System.Serializable]
        public class LeaderBoardView : ScoreboardView
        {
            [SerializeField][DebugOnly] protected Transform[] _top3 = new Transform[3];

            public LeaderBoardView(Transform transform) : base(transform, "Footer/")
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
                    _top3[idx].Find("Avatar").GetComponent<Image>().sprite = await roomStatus.LocalUserCache.GetUserAvatar(quizPlayers[idx].AvatarPath);
                    _top3[idx].Find("Name_Txt").GetComponent<TextMeshProUGUI>().text = quizPlayers[idx].Name;
                    _top3[idx].Find("Score_Txt").GetComponent<TextMeshProUGUI>().text = $"{quizPlayers[idx].Score}";
                    _top3[idx].Find("Rank_Txt").GetComponent<TextMeshProUGUI>().text = $"{quizPlayers[idx].Rank}";
                }

                await UpdateData(roomStatus, quizPlayers.Skip(3).ToArray());
            }
        }

        private GameStore _gameStore;
        private AudioPoolManager _audioPoolManager;
        private VirtualRoomPresenter _virtualRoomPresenter;
        private IUserDataController _userDataController;

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
            _gameStore = gameStore;
            _audioPoolManager = (AudioPoolManager)container.Resolve<IReadOnlyList<IPoolManager>>().ElementAt((int)PoolName.Audio);
            _virtualRoomPresenter = container.Resolve<VirtualRoomPresenter>();
            _userDataController = container.Resolve<IUserDataController>();
        }

        private void GetReferences()
        {
            _object3DContainer = transform.Find("3D_Renderer/Object");

            _headerView = new(transform.Find("Canvas/Header"));
            _previewView = new(transform.Find("Canvas/Preview"));
            _questionView = new(transform.Find("Canvas/Question"));
            _scoreboardView = new(transform.Find("Canvas/Scoreboard"));
            _leaderBoardView = new(transform.Find("Canvas/LeaderBoard"));
        }

        public override void OnReady()
        {
            GetReferences();

            Refresh();
        }

        public void OnDoneShowPreview()
        {
        }

        public void OnEndQuestion()
        {
        }

        public void Refresh()
        {
        }
    }
}
