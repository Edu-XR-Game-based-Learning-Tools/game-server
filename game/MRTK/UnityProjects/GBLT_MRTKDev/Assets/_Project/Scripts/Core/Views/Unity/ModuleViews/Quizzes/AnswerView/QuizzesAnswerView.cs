using Core.Business;
using Core.Extension;
using Core.Framework;
using Core.Module;
using Core.Utility;
using Cysharp.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.UX;
using Shared.Network;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using static Core.View.QuizzesQuestionView;

namespace Core.View
{
    public class QuizzesAnswerView : UnityView
    {
        [System.Serializable]
        public class LoadingContainer : SubView
        {
            public LoadingContainer(Transform transform, IObjectResolver container) : base(transform, container)
            {
            }
        }

        [System.Serializable]
        public class AnswerView : ObjectQuizVisual
        {
            [SerializeField][DebugOnly] private TextMeshProUGUI _questionTxt;

            [SerializeField][DebugOnly] private PressableButton[] _optionBtns;
            bool _isAnswered = false;

            public AnswerView(Transform transform, IObjectResolver container) : base(transform, container)
            {
                _questionTxt = transform.Find("Header/Question_Txt").GetComponent<TextMeshProUGUI>();

                var optionParent = transform.Find("Footer/Options");
                _optionBtns = new PressableButton[optionParent.childCount];
                for (int idx = 0; idx < optionParent.childCount; idx++)
                {
                    _optionBtns[idx] = optionParent.GetChild(idx).GetComponent<PressableButton>();
                }

                Init();
                RegisterEvents();
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

            private void Init()
            {
                _questionTxt.text = "Question?";
                EnableOption();
            }

            public override void RegisterEvents()
            {
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
                    });
                }
            }

            public async UniTask UpdateContent(QuizDto data)
            {
                _isAnswered = false;
                _questionTxt.text = data.Question;
                for (int idx = 0; idx < _optionBtns.Length; idx++)
                {
                    _optionBtns[idx].SetActive(idx < data.Answers.Length);
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
            [SerializeField][DebugOnly] private Image _correctImg;
            [SerializeField][DebugOnly] private Image _incorrectImg;
            [SerializeField][DebugOnly] private Transform _scoreContainer;

            public ScoreView(Transform transform, IObjectResolver container) : base(transform, container)
            {
                _resultTxt = transform.Find("Layout/Content/Result_Txt").GetComponent<TextMeshProUGUI>();
                _correctImg = transform.Find("Layout/Content/Icon/Incorrect").GetComponent<Image>();
                _incorrectImg = transform.Find("Layout/Content/Icon/Correct").GetComponent<Image>();
                _scoreContainer = transform.Find("Layout/Content/Score");
            }

            public void UpdateContent(bool isCorrect = false, int score = 0)
            {
                _resultTxt.text = isCorrect ? "CORRECT" : "INCORRECT";
                _resultTxt.color = isCorrect ? Color.green : Color.red;
                _correctImg.SetActive(isCorrect);
                _incorrectImg.SetActive(!isCorrect);
                _scoreContainer.SetActive(isCorrect);
                _incorrectImg.GetComponentInChildren<TextMeshProUGUI>().text = $"+ {score}";
            }
        }

        [System.Serializable]
        public class RankView : SubView
        {
            [SerializeField][DebugOnly] private TextMeshProUGUI _nameTxt;
            [SerializeField][DebugOnly] private Image _avatarImg;
            [SerializeField][DebugOnly] private TextMeshProUGUI _scoreTxt;
            [SerializeField][DebugOnly] private TextMeshProUGUI _rankTxt;

            public RankView(Transform transform, IObjectResolver container) : base(transform, container)
            {
                _nameTxt = transform.Find("Layout/Content/Name_Txt").GetComponent<TextMeshProUGUI>();
                _avatarImg = transform.Find("Layout/Content/Icon").GetComponent<Image>();
                _scoreTxt = transform.Find("Layout/Content/Score_Txt").GetComponent<TextMeshProUGUI>();
                _rankTxt = transform.Find("Layout/Content/Rank_Txt").GetComponent<TextMeshProUGUI>();
            }

            public void UpdateContent(string name, Sprite sprite, int score, int rank)
            {
                _nameTxt.text = name;
                _avatarImg.sprite = sprite;
                _scoreTxt.text = $"Score: {score}";
                _rankTxt.text = $"Rank: {rank}";
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

            transform.SetActive(false);
        }

        private void GetReferences()
        {
            _object3DContainer = transform.Find("3D_Renderer/Object");

            _loadingContainer = new LoadingContainer(transform.Find("Canvas/LoadingContainer"), _container);
            _answerView = new AnswerView(transform.Find("Canvas/Answer"), _container);
            _scoreView = new ScoreView(transform.Find("Canvas/Score"), _container);
            _rankView = new RankView(transform.Find("Canvas/Rank"), _container);
        }

        public override void OnReady()
        {
            GetReferences();

            Refresh();
        }

        public async UniTask SetupQuestion()
        {
            QuizzesStatusResponse status = _userDataController.ServerData.RoomStatus.InGameStatus;
            int questionIdx = (int)status.JoinQuizzesData.CurrentQuestionIdx;
            _ = _answerView.UpdateContent(status.QuizCollection.Quizzes[questionIdx]);
            _object3DContainer.Find("Object").GetComponent<MeshFilter>().sharedMesh = await MeshFromURL.FetchModel(status.QuizCollection.Quizzes[questionIdx].Model);
        }

        public void OnStart()
        {
            transform.SetActive(true);
            _loadingContainer.Transform.SetActive(true);
            _answerView.Transform.SetActive(false);
            _scoreView.Transform.SetActive(false);
            _rankView.Transform.SetActive(false);

            _ = SetupQuestion();
        }

        public void OnDonePreview()
        {
            _loadingContainer.Transform.SetActive(false);
            _answerView.Transform.SetActive(true);
        }

        public void OnEndQuestion()
        {
            _answerView.Transform.SetActive(false);
            _scoreView.Transform.SetActive(true);
        }

        public async void OnNextQuestion()
        {
            await SetupQuestion();
            _scoreView.Transform.SetActive(false);
            _loadingContainer.Transform.SetActive(true);
        }

        public void OnEndQuiz()
        {
            _scoreView.Transform.SetActive(false);
            _rankView.Transform.SetActive(true);
        }

        public void Refresh()
        {
        }
    }
}
