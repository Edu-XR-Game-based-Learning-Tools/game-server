using Core.Business;
using Core.Extension;
using Core.Framework;
using Core.Utility;
using Cysharp.Threading.Tasks;
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
    public class QuizzesAnswerView : UnityView, IQuizzesManualHandling
    {
        [System.Serializable]
        public class LoadingContainer : SubView
        {
            public LoadingContainer(Transform transform, IObjectResolver container, Transform viewRoot, System.Action onBack = null) : base(transform, container, viewRoot, onBack)
            {
            }
        }

        [System.Serializable]
        public class AnswerView : ObjectQuizVisual
        {
            private readonly QuizzesHub _quizzesHub;

            [SerializeField][DebugOnly] private TextMeshProUGUI _questionTxt;

            [SerializeField][DebugOnly] private PressableButton[] _optionBtns;
            bool _isAnswered = false;

            public AnswerView(Transform transform, IObjectResolver container, Transform viewRoot, System.Action onBack = null) : base(transform, container, viewRoot, onBack)
            {
                _quizzesHub = container.Resolve<QuizzesHub>();

                _questionTxt = transform.Find("Header/Question_Txt").GetComponent<TextMeshProUGUI>();

                var optionParent = transform.Find("Footer/Options");
                _optionBtns = new PressableButton[optionParent.childCount];
                for (int idx = 0; idx < optionParent.childCount; idx++)
                {
                    _optionBtns[idx] = optionParent.GetChild(idx).GetComponent<PressableButton>();
                }

                Init();
            }

            private void EnableOption(bool[] options = null)
            {
                options ??= Enumerable.Repeat(true, _optionBtns.Length).ToArray();

                for (int idx = 0; idx < _optionBtns.Length; idx++)
                {
                    Color color = (idx >= options.Length || options[idx]) ? new Color(1f, 1f, 1f) : new Color(70f / 255, 70f / 255, 70f / 255);
                    _optionBtns[idx].transform.Find("Backplate").GetComponent<Image>().color = color;
                }
            }

            public override void Init()
            {
                _questionTxt.text = "Question?";
                EnableOption();
                base.Init();
            }

            public override void RegisterEvents()
            {
                base.RegisterEvents();

                for (int idx = 0; idx < _optionBtns.Length; idx++)
                {
                    int index = idx;
                    _optionBtns[idx].OnClicked.AddListener(() =>
                    {
                        if (_isAnswered) return;
                        _isAnswered = true;
                        bool[] options = new bool[_optionBtns.Length];
                        options[index] = true;
                        EnableOption(options);
                        _quizzesHub.Answer(new AnswerData()
                        {
                            AnswerIdx = index,
                        });
                    });
                }
            }

            public async UniTask UpdateContent(QuizDto data)
            {
                _isAnswered = false;
                _questionTxt.text = data.Question;
                EnableOption();
                for (int idx = 0; idx < _optionBtns.Length; idx++)
                {
                    _optionBtns[idx].SetActive(idx < data.Answers.Length && !data.Answers[idx].IsNullOrEmpty());
                    if (idx >= data.Answers.Length) continue;
                    _optionBtns[idx].transform.Find("Frontplate/AnimatedContent/Text").GetComponent<TextMeshProUGUI>().text = data.Answers[idx];
                }
                _imageObject2D.sprite = await IMG2Sprite.FetchImageSprite(data.Image);
            }
        }

        [System.Serializable]
        public class ScoreView : SubView
        {
            [SerializeField][DebugOnly] private TextMeshProUGUI _resultTxt;
            [SerializeField][DebugOnly] private Transform _correctImg;
            [SerializeField][DebugOnly] private Transform _incorrectImg;
            [SerializeField][DebugOnly] private Transform _scoreContainer;

            protected UserDataController _userDataController;

            public ScoreView(Transform transform, IObjectResolver container, Transform viewRoot, System.Action onBack = null) : base(transform, container, viewRoot, onBack)
            {
                _userDataController = container.Resolve<IUserDataController>() as UserDataController;

                _resultTxt = transform.Find("Layout/Content/Result_Txt").GetComponent<TextMeshProUGUI>();
                _correctImg = transform.Find("Layout/Content/Icon/Correct");
                _incorrectImg = transform.Find("Layout/Content/Icon/Incorrect");
                _scoreContainer = transform.Find("Layout/Content/Score");

                Init();
            }

            private float _lastScore = 0f;

            public void Init()
            {
                _lastScore = 0;
            }

            public void UpdateContent()
            {
                QuizzesUserData data = _userDataController.ServerData.RoomStatus.InGameStatus.Self;
                float changedScore = data.Score - _lastScore;
                _resultTxt.text = changedScore > 0 ? "CORRECT" : "INCORRECT";
                _resultTxt.color = changedScore > 0 ? Color.green : Color.red;
                _correctImg.SetActive(changedScore > 0);
                _incorrectImg.SetActive(!(changedScore > 0));

                _scoreContainer.SetActive(changedScore > 0);
                _scoreContainer.GetComponentInChildren<TextMeshProUGUI>().text = $"+ {changedScore}";

                _lastScore = data.Score;
            }
        }

        [System.Serializable]
        public class RankView : SubView
        {
            [SerializeField][DebugOnly] private TextMeshProUGUI _nameTxt;
            [SerializeField][DebugOnly] private Image _avatarImg;
            [SerializeField][DebugOnly] private TextMeshProUGUI _scoreTxt;
            [SerializeField][DebugOnly] private TextMeshProUGUI _rankTxt;

            protected UserDataController _userDataController;

            public RankView(Transform transform, IObjectResolver container, Transform viewRoot, System.Action onBack = null) : base(transform, container, viewRoot, onBack)
            {
                _userDataController = container.Resolve<IUserDataController>() as UserDataController;

                _nameTxt = transform.Find("Layout/Content/Name_Txt").GetComponent<TextMeshProUGUI>();
                _avatarImg = transform.Find("Layout/Content/Icon").GetComponent<Image>();
                _scoreTxt = transform.Find("Layout/Content/Score_Txt").GetComponent<TextMeshProUGUI>();
                _rankTxt = transform.Find("Layout/Content/Rank_Txt").GetComponent<TextMeshProUGUI>();
            }

            public async UniTask UpdateContent()
            {
                QuizzesUserData data = _userDataController.ServerData.RoomStatus.InGameStatus.Self;
                _nameTxt.text = data.UserData.Name;
                _avatarImg.sprite = await _userDataController.LocalUserCache.GetSprite(data.UserData.AvatarPath);
                _scoreTxt.text = $"Score: {data.Score}";
                _rankTxt.text = $"Rank: {data.Rank}";
            }
        }

        private IObjectResolver _container;
        private GameStore _gameStore;
        private AudioPoolManager _audioPoolManager;
        private VirtualRoomPresenter _virtualRoomPresenter;
        private IUserDataController _userDataController;

        [SerializeField][DebugOnly] private Transform _object3DContainer;

        [SerializeField][DebugOnly] private LoadingContainer _loadingContainer;
        [SerializeField][DebugOnly] private AnswerView _answerView;
        [SerializeField][DebugOnly] private ScoreView _scoreView;
        [SerializeField][DebugOnly] private RankView _rankView;

        public void Init(IObjectResolver container)
        {
            _container = container;
            _gameStore = container.Resolve<GameStore>();
            _audioPoolManager = (AudioPoolManager)container.Resolve<IReadOnlyList<IPoolManager>>().ElementAt((int)PoolName.Audio);
            _virtualRoomPresenter = container.Resolve<VirtualRoomPresenter>();
            _userDataController = container.Resolve<IUserDataController>();

            OnReady();
            transform.SetActive(false);
        }

        private void GetReferences()
        {
            _object3DContainer = transform.Find("3D_Renderer/Object");

            _loadingContainer = new LoadingContainer(transform.Find("Canvas/LoadingContainer"), _container, transform);
            _answerView = new AnswerView(transform.Find("Canvas/Answer"), _container, transform);
            _scoreView = new ScoreView(transform.Find("Canvas/Score"), _container, transform);
            _rankView = new RankView(transform.Find("Canvas/Rank"), _container, transform);
        }

        public override void OnReady()
        {
            GetReferences();

            Refresh();
        }

        public async UniTask SetupQuestion()
        {
            _answerView.Init();
            QuizzesStatusResponse status = _userDataController.ServerData.RoomStatus.InGameStatus;
            int questionIdx = status.JoinQuizzesData.CurrentQuestionIdx;
            if (questionIdx >= status.QuizCollection.Quizzes.Length) return;
            _ = _answerView.UpdateContent(status.QuizCollection.Quizzes[questionIdx]);
            _object3DContainer.Find("Mesh").GetComponent<MeshFilter>().sharedMesh = await MeshFromURL.FetchModel(status.QuizCollection.Quizzes[questionIdx].Model);
        }

        #region Handle Hub Response

        public async void OnStart()
        {
            _scoreView.Init();
            await SetupQuestion();

            transform.SetActive(true);
            _loadingContainer.Transform.SetActive(true);
            _answerView.Transform.SetActive(false);
            _scoreView.Transform.SetActive(false);
            _rankView.Transform.SetActive(false);
        }

        public void OnDonePreview()
        {
            _loadingContainer.Transform.SetActive(false);
            _answerView.Transform.SetActive(true);
        }


        public void OnEndQuestion()
        {
            _scoreView.UpdateContent();

            _answerView.Transform.SetActive(false);
            _scoreView.Transform.SetActive(true);
        }

        public async void OnNextQuestion()
        {
            await SetupQuestion();

            _answerView.Transform.SetActive(false);
            _scoreView.Transform.SetActive(false);
            _loadingContainer.Transform.SetActive(true);
        }

        public void OnEndQuiz()
        {
            _ = _rankView.UpdateContent();

            _answerView.Transform.SetActive(false);
            _scoreView.Transform.SetActive(false);
            _rankView.Transform.SetActive(true);
        }

        public void OnEndSession()
        {
            transform.SetActive(false);
        }

        #endregion Handle Hub Response

        public void Refresh()
        {
        }
    }
}
