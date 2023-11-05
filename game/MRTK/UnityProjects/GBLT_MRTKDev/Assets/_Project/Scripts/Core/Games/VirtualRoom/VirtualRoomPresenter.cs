using Core.Business;
using Core.EventSignal;
using Core.Extension;
using Core.Module;
using Core.Utility;
using Core.View;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Shared;
using Shared.Network;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Core.Framework
{
    public class VirtualRoomPresenter : MonoBehaviour
    {
        private IObjectResolver _container;
        private GameStore _gameStore;
        private IDefinitionManager _definitionManager;
        private IBundleLoader _bundleLoader;
        private IUserDataController _userDataController;

        [Inject]
        private readonly IPublisher<OnVirtualRoomTickSignal> _onUserTransformChangePublisher;

        [Inject]
        protected readonly IPublisher<ShowPopupSignal> _showPopupPublisher;

        [SerializeField][DebugOnly] private Transform _classRoom;
        [SerializeField][DebugOnly] private ClassRoomDefinition _classRoomDefinition;
        [SerializeField][DebugOnly] private Transform _teacherSeatTransform;
        [SerializeField][DebugOnly] private Transform _teacherCharacter;
        [SerializeField][DebugOnly] private Transform[] _studentSeatTransforms;
        [SerializeField][DebugOnly] private Transform[] _studentCharacters = new Transform[0];

        [SerializeField][DebugOnly] private Transform _MRTKRig;

        [Header("Share Screen")]
        [SerializeField][DebugOnly] private Camera _shareScreenCam;
        [SerializeField][DebugOnly] private RenderTexture _screenRenderTexture;
        [SerializeField][DebugOnly] private Camera _quizzesShareCam;
        [SerializeField][DebugOnly] private RenderTexture _quizzesShareRenderTexture;

        [SerializeField][DebugOnly] private Texture2D _shareTexture;
        [SerializeField][DebugOnly] private Texture2D _screenTex;

        [Inject]
        public void Construct(IObjectResolver container)
        {
            _container = container;

            _gameStore = container.Resolve<GameStore>();
            _definitionManager = container.Resolve<IDefinitionManager>();
            _bundleLoader = container.Resolve<IReadOnlyList<IBundleLoader>>().ElementAt((int)BundleLoaderName.Addressable);
            _userDataController = container.Resolve<IUserDataController>();
        }

        public async void Init()
        {
            _classRoomDefinition = await _definitionManager.GetDefinition<ClassRoomDefinition>("0");
            _classRoom = GameObject.Find("ClassRoom").transform;

            _teacherSeatTransform = _classRoom.Find("TeacherSeat");
            _quizzesShareCam = _teacherSeatTransform.Find("ShareScreenCam").GetComponent<Camera>();
            _quizzesShareRenderTexture = _quizzesShareCam.targetTexture;
            var studentSeatContainer = _classRoom.Find("Seats");
            _studentSeatTransforms = new Transform[studentSeatContainer.childCount];
            for (int idx = 0; idx < studentSeatContainer.childCount; idx++)
                _studentSeatTransforms[idx] = studentSeatContainer.GetChild(idx);

            _MRTKRig = GameObject.Find("MRTK XR Rig").transform;
            _shareScreenCam = GameObject.Find("MRTK XR Rig/Camera Offset/Main Camera/ShareScreenCam").GetComponent<Camera>();
            _screenRenderTexture = _shareScreenCam.targetTexture;
            _screenTex = new Texture2D(_screenRenderTexture.width, _screenRenderTexture.height);

            _classRoom.SetActive(false);

            Camera.onPostRender += OnPostRenderCallback;
        }

        #region Setup Environment

        #region Spawn First Enter

        private async UniTask<Transform> SpawnCharacter(PublicUserData user = null, bool isSelf = false)
        {
            var data = user ?? _userDataController.ServerData.RoomStatus.RoomStatus.Self;

            var prefabPath = !string.IsNullOrEmpty(data.ModelPath)
                ? data.ModelPath : Defines.PrefabKey.DefaultRoomModel;
            Transform parent = GetCharacterParent(user);
            if (parent.childCount == 0 || prefabPath != parent.GetChild(0).name)
            {
                DestroyCharacter(data);

                GameObject prefab = await ((UserDataController)_userDataController).LocalUserCache.GetModel(prefabPath);
                var obj = _container.Instantiate(prefab, parent).transform;

                obj.transform.eulerAngles = data.HeadRotation.ToVector3();
                if (data.IsHost) _teacherCharacter = obj;
                else _studentCharacters[data.Index] = obj;
            }

            if (isSelf)
            {
                _MRTKRig.position = parent.GetChild(0).Find("EyeCamPosition").position;
                _MRTKRig.rotation = parent.GetChild(0).Find("EyeCamPosition").rotation;
            }

            if (data.IsHost) return _teacherCharacter;
            else return _studentCharacters[data.Index];
        }

        private async UniTask UpdateCharacterCanvas(PublicUserData user, Transform character)
        {
            character.Find("Canvas/Side/Title").GetComponent<TextMeshProUGUI>().text = user.Name;
            character.Find("Canvas/Side/Image").GetComponent<Image>().sprite = await ((UserDataController)_userDataController).LocalUserCache.GetSprite(user.AvatarPath);

            character.Find("Canvas/Flip/Title").GetComponent<TextMeshProUGUI>().text = user.Name;
            character.Find("Canvas/Flip/Image").GetComponent<Image>().sprite = await ((UserDataController)_userDataController).LocalUserCache.GetSprite(user.AvatarPath);
        }

        private async UniTask UpdateModuleUI(PublicUserData user, bool isShow = true)
        {
            if (_gameStore.GState.HasModel<RoomStatusModel>())
            {
                await (await _gameStore.GetOrCreateModule<RoomStatus, RoomStatusModel>(moduleName: ModuleName.RoomStatus)).ViewContext.View.GetComponent<RoomStatusView>().UpdateCharacter(user, isShow);
            }

            if (_gameStore.GState.HasModel<QuizzesRoomStatusModel>())
            {
                await (await _gameStore.GetOrCreateModule<QuizzesRoomStatus, QuizzesRoomStatusModel>(moduleName: ModuleName.QuizzesRoomStatus)).ViewContext.View.GetComponent<QuizzesRoomStatusView>().UpdateCharacter(user, isShow);
            }
        }

        private async UniTask UpdateCharacter(PublicUserData user)
        {
            await UpdateModuleUI(user);
            var character = await SpawnCharacter(user, _userDataController.ServerData.RoomStatus.RoomStatus.Self.Index == user.Index);
            await UpdateCharacterCanvas(user, character);
        }

        public async UniTask Spawn() // 24 - 48
        {
            foreach (Transform transform in _studentCharacters)
                if (transform != null)
                    Destroy(transform.gameObject);
            _studentCharacters = new Transform[_studentSeatTransforms.Length];

            foreach (var user in _userDataController.ServerData.RoomStatus.RoomStatus.AllInRoom) await UpdateCharacter(user);

            _classRoom.SetActive(true);
        }

        #endregion Spawn First Enter

        public async void OnJoin(PublicUserData user)
        {
            await UpdateCharacter(user);
        }

        public Transform GetCharacterParent(PublicUserData user)
        {
            Transform userSeat = user.IsHost ? _teacherSeatTransform : _studentSeatTransforms[user.Index];
            Transform parent = userSeat.Find("CharPosition");
            return parent;
        }

        private Transform DestroyCharacter(PublicUserData user)
        {
            Debug.Log($"DestroyCharacter {user.IsHost}");
            Transform parent = GetCharacterParent(user);

            foreach (Transform child in parent)
                Destroy(child.gameObject);
            if (user.IsHost) _teacherCharacter = null;
            else _studentCharacters[user.Index] = null;

            return parent;
        }

        public void OnLeave(PublicUserData user)
        {
            Debug.Log($"OnLeave");
            DestroyCharacter(user);
            _ = UpdateModuleUI(user, false);
            if (_userDataController.ServerData.RoomStatus.RoomStatus.Self.Index == user.Index)
                Clean();
        }

        #endregion Setup Environment

        public void Clean()
        {
            foreach (Transform child in _studentCharacters)
                if (child != null) Destroy(child.gameObject);
            if (_teacherCharacter != null) Destroy(_teacherCharacter.gameObject);

            _classRoom.SetActive(false);
        }

        #region Sharing
        [SerializeField][DebugOnly] bool _isSharingQuizzesCache = false;
        [SerializeField][DebugOnly] bool _isSharingCache = false;
        private void UpdateShareTexture()
        {
            if (!_userDataController.ServerData.RoomStatus.RoomStatus.Self.IsHost) return;

            if (_userDataController.ServerData.IsSharingQuizzesGame)
            //&& _userDataController.ServerData.IsSharingQuizzesGame != _isSharingQuizzesCache)
            {
                _shareTexture = _quizzesShareRenderTexture.ToTexture2D();
            }
            else if (_userDataController.ServerData.IsSharing)
            //&& _userDataController.ServerData.IsSharing != _isSharingCache)
            {
                _shareTexture = _screenRenderTexture.ToTexture2D();
            }
            _isSharingQuizzesCache = _userDataController.ServerData.IsSharingQuizzesGame;
            _isSharingCache = _userDataController.ServerData.IsSharing;
        }

        [SerializeField][DebugOnly] bool _isStillNoTeacher = false;
        [SerializeField] float _delaySyncDuration = 1f;
        [SerializeField][DebugOnly] float _delaySyncData;
        private void PublishTickData()
        {
            if (_teacherCharacter == null || !_teacherCharacter.gameObject.activeInHierarchy)
            {
                if (!_isStillNoTeacher)
                    if (_teacherSeatTransform.Find("CharPosition").childCount > 0)
                        _teacherCharacter = _teacherSeatTransform.Find("CharPosition").GetChild(0);
                if (_teacherCharacter == null) _isStillNoTeacher = true;
                else PublishTickData();
                return;
            }
            _isStillNoTeacher = false;

            _delaySyncData -= Time.deltaTime;
            if (_delaySyncData > 0)
                return;

            _delaySyncData = _delaySyncDuration;
            UpdateShareTexture();

            _onUserTransformChangePublisher.Publish(new OnVirtualRoomTickSignal(new VirtualRoomTickData
            {
                HeadRotation = _teacherCharacter.eulerAngles.ToVec3D(),
                Texture = _shareTexture == null ? null : _shareTexture.EncodeToJPG(),
                IsSharing = _userDataController.ServerData.IsSharing,
                IsSharingQuizzesGame = _userDataController.ServerData.IsSharingQuizzesGame,
            }));
        }

        private void OnPostRenderCallback(Camera cam)
        {
            if (!_userDataController.ServerData.IsInRoom) return;
            PublishTickData();
        }
        #endregion Sharing

        private void Update()
        {
            if (!_userDataController.ServerData.IsInRoom) return;
        }

        private void OnDestroy()
        {
            Camera.onPostRender -= OnPostRenderCallback;
        }

        public void OnTransform(PublicUserData user)
        {
            Transform userChar = user.IsHost ? _teacherCharacter : _studentCharacters[user.Index];
            if (userChar == null) return;

            userChar.transform.eulerAngles = user.HeadRotation.ToVector3();
        }

        public void OnRoomTick(VirtualRoomTickResponse response)
        {
            OnTransform(response.User);
            if (response.Texture != null)
                _screenTex.LoadImage(response.Texture);
        }

        public async void OnUpdateAvatar(PublicUserData user)
        {
            await UpdateCharacter(user);
        }

        #region Quizzes

        private QuizzesQuestionView _quizzesQuestionView;
        private QuizzesAnswerView _quizzesAnswerView;

        private PrivateUserData _self => _userDataController.ServerData.RoomStatus.RoomStatus.Self;

        private Transform GetSelfTableUI()
        {
            Transform userSeat = _self.IsHost ? _teacherSeatTransform : _studentSeatTransforms[_self.Index];
            Transform ui = userSeat.Find("UI");
            return ui;
        }

        public void OnJoinQuizzes(QuizzesUserData _)
        {
            if (_self.IsHost)
            {
                _quizzesQuestionView = GetSelfTableUI().GetComponent<QuizzesQuestionView>();
            }
            else
            {
                _quizzesAnswerView = GetSelfTableUI().GetComponent<QuizzesAnswerView>();
            }
        }

        public void OnLeaveQuizzes(QuizzesUserData _)
        {
            if (_self.IsHost)
                OnEndQuizQuizzes();
        }

        #region Only Host

        public async UniTask StartGame(QuizCollectionDto collection)
        {
            await _quizzesQuestionView.StartGame(collection);
        }

        public void OnAnswerQuizzes(AnswerData data)
        {
            _quizzesQuestionView.OnAnswer(data);
        }

        #endregion Only Host

        #region Only player

        public void OnStartQuizzes(QuizzesStatusResponse _)
        {
            _quizzesAnswerView.OnStart();
        }

        public void OnDonePreviewQuizzes()
        {
            _quizzesAnswerView.OnDonePreview();
        }

        public void OnEndQuestionQuizzes()
        {
            _quizzesAnswerView.OnEndQuestion();
        }

        public void OnNextQuestionQuizzes(QuizzesStatusResponse _)
        {
            _quizzesAnswerView.OnNextQuestion();
        }

        public void OnEndQuizQuizzes()
        {
            _quizzesAnswerView.OnEndQuiz();
        }

        #endregion Only player

        #endregion Quizzes
    }
}
