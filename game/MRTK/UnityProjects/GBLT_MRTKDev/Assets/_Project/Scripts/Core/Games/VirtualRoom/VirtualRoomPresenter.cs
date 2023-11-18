using Core.Business;
using Core.EventSignal;
using Core.Extension;
using Core.Module;
using Core.Utility;
using Core.View;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Models;
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

        [SerializeField][DebugOnly] private Material _screenMat;
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
            _screenTex = new(_screenRenderTexture.width, _screenRenderTexture.height);

            _screenMat = _classRoom.Find("Environment/Projector/Screen/16:9").GetComponent<Renderer>().material;
            _screenMat.SetColor("_EmissionColor", new Color(0f, 0f, 0f, 1f));
            _screenMat.SetTexture("_EmissionMap", null);

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

            var characterObject = parent.GetChild(0);
            if (isSelf)
            {
                _MRTKRig.position = characterObject.Find("EyeCamPosition").position;
                _MRTKRig.eulerAngles = characterObject.Find("EyeCamPosition").eulerAngles + Vector3.up * (data.IsHost ? -20f : 180f);
            }

            characterObject.Find("Head").ChangeLayersRecursively(isSelf ? "SelfModel" : "Default");

            return characterObject;
        }

        private async UniTask UpdateCharacterCanvas(PublicUserData user, Transform character)
        {
            character.Find("Canvas/Side/Title").GetComponent<TextMeshProUGUI>().text = user.Name;
            character.Find("Canvas/Side/Image").GetComponent<Image>().sprite = await ((UserDataController)_userDataController).LocalUserCache.GetSprite(user.AvatarPath);

            character.Find("Canvas/Flip/Title").GetComponent<TextMeshProUGUI>().text = user.Name;
            character.Find("Canvas/Flip/Image").GetComponent<Image>().sprite = await ((UserDataController)_userDataController).LocalUserCache.GetSprite(user.AvatarPath);
        }

        private void UpdateQuizzesStatusModuleUI()
        {
            if (_gameStore.GState.HasModel<QuizzesRoomStatusModel>())
            {
                var model = _gameStore.GState.GetModel<QuizzesRoomStatusModel>();
                if (model.Module.ViewContext.View.activeInHierarchy) model.Refresh();
            }
        }

        private void UpdateRoomStatusModuleUI()
        {
            if (_gameStore.GState.HasModel<RoomStatusModel>())
            {
                var model = _gameStore.GState.GetModel<RoomStatusModel>();
                if (model.Module.ViewContext.View.activeInHierarchy) model.Refresh();
            }

            UpdateQuizzesStatusModuleUI();
        }

        private async UniTask UpdateCharacter(PublicUserData user)
        {
            UpdateRoomStatusModuleUI();
            var character = await SpawnCharacter(user, _userDataController.ServerData.RoomStatus.RoomStatus.Self.ConnectionId == user.ConnectionId);
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
            Debug.Log($"OnLeave {user.Index}");
            if (!_userDataController.ServerData.IsInRoom || _userDataController.ServerData.RoomStatus.RoomStatus.Self.ConnectionId == user.ConnectionId)
                Clean();
            else
            {
                DestroyCharacter(user);
                UpdateRoomStatusModuleUI();
            }
        }

        #endregion Setup Environment

        public async void Clean()
        {
            foreach (Transform child in _studentCharacters)
                if (child != null) Destroy(child.gameObject);
            if (_teacherCharacter != null) Destroy(_teacherCharacter.gameObject);

            _gameStore.GState.RemoveModel<RoomStatusModel>();
            _gameStore.GState.RemoveModel<QuizzesRoomStatusModel>();
            (await _gameStore.GetOrCreateModel<LandingScreen, LandingScreenModel>(
                moduleName: ModuleName.LandingScreen)).Refresh();

            _classRoom.SetActive(false);
        }

        #region Sharing
        [SerializeField] bool _isSharingQuizzesCache = false;
        [SerializeField] bool _isSharingCache = false;
        private Texture2D GetShareTexture()
        {
            if (!_userDataController.ServerData.RoomStatus.RoomStatus.Self.IsHost) return null;

            bool isSharing = _userDataController.ServerData.IsInRoom && _userDataController.ServerData.IsSharing && !_userDataController.ServerData.IsSharingQuizzesGame;
            bool isSharingQuizzes = _userDataController.ServerData.IsInGame && _userDataController.ServerData.IsSharingQuizzesGame;

            _shareScreenCam.SetActive(isSharing);
            _quizzesShareCam.SetActive(isSharingQuizzes);
            if (isSharingQuizzes)
                return _quizzesShareRenderTexture.ToTexture2D();
            else if (isSharing)
                return _screenRenderTexture.ToTexture2D();
            else return null;
        }

        [SerializeField][DebugOnly] bool _isStillNoTeacher = false;
        [SerializeField] float _delaySyncDuration = 0.25f;
        [SerializeField][DebugOnly] float _delaySyncData;
        [SerializeField] float _delaySharingSyncDuration = 1f;
        [SerializeField][DebugOnly] float _delaySharingSyncData;
        private void PublishRoomTickData()
        {
            _delaySyncData -= Time.deltaTime;
            if (_delaySyncData > 0)
                return;
            _delaySyncData = _delaySyncDuration;

            var xrCamEuler = _shareScreenCam.transform.parent.eulerAngles;
            _onUserTransformChangePublisher.Publish(new OnVirtualRoomTickSignal(tickData: new VirtualRoomTickData
            {
                HeadRotation = new Vec3D(-xrCamEuler.x, xrCamEuler.y + 180f, xrCamEuler.z),
            }));
        }

        private void PublishSharingTickData()
        {
            _delaySharingSyncData -= Time.deltaTime;
            if (_delaySharingSyncData > 0)
                return;
            _delaySharingSyncData = _delaySharingSyncDuration;

            if (_isSharingCache == _userDataController.ServerData.IsSharing && _userDataController.ServerData.IsSharingQuizzesGame == _isSharingQuizzesCache)
                return;

            if (!_userDataController.ServerData.RoomStatus.RoomStatus.Self.IsHost) return;

            _isSharingCache = _userDataController.ServerData.IsSharing;
            _isSharingQuizzesCache = _userDataController.ServerData.IsSharingQuizzesGame;

            var shareTexture = GetShareTexture();

            _onUserTransformChangePublisher.Publish(new OnVirtualRoomTickSignal(sharingTickData: new SharingTickData
            {
                Texture = shareTexture == null ? null : shareTexture.EncodeToJPG(),
                IsSharing = _userDataController.ServerData.IsSharing,
                IsSharingQuizzesGame = _userDataController.ServerData.IsSharingQuizzesGame,
            }));

            if (shareTexture != null) Destroy(shareTexture);
        }

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

            PublishRoomTickData();
            PublishSharingTickData();
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
            if (_screenTex != null)
            {
                Destroy(_screenTex);
                _screenTex = null;
            }
        }

        public void OnTransform(PublicUserData user)
        {
            Transform userModelParent = GetCharacterParent(user);
            if (userModelParent == null || userModelParent.childCount == 0) return;

            userModelParent.GetChild(0).Find("Head").transform.eulerAngles = user.HeadRotation.ToVector3();
        }

        public void OnRoomTick(VirtualRoomTickResponse response)
        {
            OnTransform(response.User);
        }

        public void OnSharingTick(SharingTickData response)
        {
            if (response.Texture == null)
            {
                _screenMat.SetColor("_EmissionColor", new Color(0f, 0f, 0f, 1f));
                _screenMat.SetTexture("_EmissionMap", null);
                return;
            }

            _screenTex.LoadImage(response.Texture);
            _screenMat.SetColor("_EmissionColor", new Color(1f, 1f, 1f, 1f) * 2.75f);
            _screenMat.SetTexture("_EmissionMap", _screenTex);
        }

        public async void OnUpdateAvatar(PublicUserData user)
        {
            await UpdateCharacter(user);
        }

        #region Quizzes

        private QuizzesQuestionView _quizzesQuestionView;
        private QuizzesAnswerView _quizzesAnswerView;

        private QuizzesUserData Self => _userDataController.ServerData.RoomStatus.InGameStatus.Self;

        private Transform GetSelfTableUI()
        {
            Transform userSeat = Self.IsHost ? _teacherSeatTransform : _studentSeatTransforms[Self.Index];
            Transform ui = userSeat.Find("UI");
            return ui;
        }

        public void OnSelfJoinQuizzes()
        {
            if (Self.IsHost)
            {
                _quizzesQuestionView = GetSelfTableUI().GetComponent<QuizzesQuestionView>();
                _quizzesQuestionView.Init(_container);
            }
            else
            {
                _quizzesAnswerView = GetSelfTableUI().GetComponent<QuizzesAnswerView>();
                _quizzesAnswerView.Init(_container);
            }
        }

        public void OnJoinQuizzes(QuizzesUserData user)
        {
            Debug.Log($"OnJoinQuizzes {user.Index}");
            UpdateQuizzesStatusModuleUI();
        }

        private IQuizzesManualHandling GetCorrespondingQuizzesUI()
        {
            return Self.IsHost ? _quizzesQuestionView : _quizzesAnswerView;
        }

        private async void CleanQuizzesTool()
        {
            if (_quizzesQuestionView != null) _quizzesQuestionView.OnEndSession();
            if (_quizzesAnswerView != null) _quizzesAnswerView.OnEndSession();

            _gameStore.GState.RemoveModel<QuizzesRoomStatusModel>();
            var model = await _gameStore.GetOrCreateModel<RoomStatus, RoomStatusModel>(moduleName: ModuleName.RoomStatus);
            model.Refresh();
        }

        public void OnLeaveQuizzes(QuizzesUserData user)
        {
            Debug.Log($"OnLeaveQuizzes {user.Index}");
            if (!_userDataController.ServerData.IsInGame || _userDataController.ServerData.RoomStatus.InGameStatus.Self.QuizzesConnectionId == user.QuizzesConnectionId)
                CleanQuizzesTool();
            else
                UpdateQuizzesStatusModuleUI();
        }

        public void OnStartQuizzes()
        {
            _gameStore.HideCurrentModule(ModuleName.QuizzesRoomStatus);
            var quizzesPanel = GetCorrespondingQuizzesUI();
            quizzesPanel.OnStart();
        }

        public void OnDonePreviewQuizzes()
        {
            var quizzesPanel = GetCorrespondingQuizzesUI();
            quizzesPanel.OnDonePreview();
        }

        public void OnEndQuestionQuizzes()
        {
            var quizzesPanel = GetCorrespondingQuizzesUI();
            quizzesPanel.OnEndQuestion();
        }

        public void OnNextQuestionQuizzes()
        {
            var quizzesPanel = GetCorrespondingQuizzesUI();
            quizzesPanel.OnNextQuestion();
        }

        public void OnEndQuizQuizzes()
        {
            if (_userDataController.ServerData.RoomStatus.InGameStatus != null && _userDataController.ServerData.RoomStatus.InGameStatus.JoinQuizzesData.QuizzesStatus == QuizzesStatus.Pending) return;
            var quizzesPanel = GetCorrespondingQuizzesUI();
            quizzesPanel.OnEndQuiz();
        }

        public async void OnEndSessionQuizzes()
        {
            var quizzesPanel = GetCorrespondingQuizzesUI();
            quizzesPanel.OnEndSession();

            var model = await _gameStore.GetOrCreateModel<QuizzesRoomStatus, QuizzesRoomStatusModel>(moduleName: ModuleName.QuizzesRoomStatus);
            model.Refresh();
        }

        #region Only Host

        public void OnAnswerQuizzes(AnswerData data)
        {
            if (Self.IsHost) _quizzesQuestionView.OnAnswer(data);
        }

        #endregion Only Host

        #endregion Quizzes
    }
}
