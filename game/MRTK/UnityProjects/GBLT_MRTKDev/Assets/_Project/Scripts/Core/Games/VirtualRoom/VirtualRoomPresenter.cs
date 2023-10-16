using Core.Business;
using Core.EventSignal;
using Core.Extension;
using Core.Utility;
using Core.View;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Shared;
using Shared.Network;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.Framework
{
    public class VirtualRoomPresenter : ITickable
    {
        private readonly IObjectResolver _container;
        private readonly IDefinitionManager _definitionManager;
        private readonly IBundleLoader _bundleLoader;
        private readonly IUserDataController _userDataController;

        [Inject]
        private readonly IPublisher<OnVirtualRoomTickSignal> _onUserTransformChangePublisher;

        [Inject]
        protected readonly IPublisher<ShowPopupSignal> _showPopupPublisher;

        private Transform _classRoom;
        private ClassRoomDefinition _classRoomDefinition;
        private Transform _seatContainer;
        private Transform _teacherSeatTransform;
        private Transform _teacherCharacter;
        private Transform[] _studentsCharacter;

        private Transform _MRTKRig;
        private Camera _shareScreenCam;
        private RenderTexture _screenRenderTexture;
        private Material _screenMat;
        private Texture2D _screenTex;

        public VirtualRoomPresenter(
            IObjectResolver container)
        {
            _container = container;

            _definitionManager = container.Resolve<IDefinitionManager>();
            _bundleLoader = container.Resolve<IReadOnlyList<IBundleLoader>>().ElementAt((int)BundleLoaderName.Addressable);
            _userDataController = container.Resolve<IUserDataController>();
        }

        public async void Init()
        {
            _classRoomDefinition = await _definitionManager.GetDefinition<ClassRoomDefinition>("0");
            _classRoom = GameObject.Find("ClassRoom").transform;
            _seatContainer = _classRoom.Find("Seats");

            _MRTKRig = GameObject.Find("MRTK XR Rig").transform;
            _shareScreenCam = GameObject.Find("MRTK XR Rig/Camera Offset/ShareScreenCam").GetComponent<Camera>();
            _screenRenderTexture = _shareScreenCam.targetTexture;
            _screenMat = _classRoom.Find("Environment/Projector/Screen/16:9").GetComponent<MeshRenderer>().material;

            _screenTex = new Texture2D(_screenRenderTexture.width, _screenRenderTexture.height);
            _screenMat.SetTexture("_EmissionMap", _screenTex);

            _classRoom.SetActive(false);
        }

        #region Setup Environment

        #region Spawn First Enter

        private async UniTask SpawnTeacherSeat()
        {
            if (_teacherSeatTransform != null) return;

            GameObject prefab = await _bundleLoader.LoadAssetAsync<GameObject>(CoreDefines.PrefabKey.TeacherSeat);
            _teacherSeatTransform = _container.Instantiate(prefab, _classRoom).transform;
            _teacherSeatTransform.position = _classRoomDefinition.TeacherSeatPosition.ToVector3();
            _teacherSeatTransform.eulerAngles = _classRoomDefinition.TeacherSeatRotation.ToVector3();
        }

        private Vector3 CalculateSeatPosition(int index)
        {
            float indexPos = index % (_classRoomDefinition.MaxColPerRow / 2);
            float xNextPos = indexPos * _classRoomDefinition.ColSpace + _classRoomDefinition.ColSpace / 1.5f;
            float xSide = index % _classRoomDefinition.MaxColPerRow >= _classRoomDefinition.MaxColPerRow / 2 ? 1 : -1;

            float x = _classRoomDefinition.StartCenterSeatPosition.x + xNextPos * xSide;
            float z = _classRoomDefinition.StartCenterSeatPosition.z - index / _classRoomDefinition.MaxColPerRow * _classRoomDefinition.RowSpace;
            return new Vector3(x, _classRoomDefinition.StartCenterSeatPosition.y, z);
        }

        private async UniTask SpawnStudentSeats(int seatAmount)
        {
            while (_seatContainer.childCount < seatAmount)
            {
                GameObject prefab = await _bundleLoader.LoadAssetAsync<GameObject>(CoreDefines.PrefabKey.StudentSeat);
                GameObject obj = _container.Instantiate(prefab, _seatContainer);
                obj.transform.position = CalculateSeatPosition(_seatContainer.childCount - 1);
            }

            for (int idx = seatAmount; idx < _seatContainer.childCount; idx++)
                _seatContainer.GetChild(idx).SetActive(false);
        }

        private async UniTask SetupCharacter(PublicUserData user = null)
        {
            var data = user ?? _userDataController.ServerData.RoomStatus.RoomStatus.Self;
            var prefabPath = !string.IsNullOrEmpty(data.ModelPath)
                ? data.ModelPath : Defines.PrefabKey.DefaultRoomModel;

            Transform parent = OnLeave(data);

            GameObject prefab = await _bundleLoader.LoadAssetAsync<GameObject>(prefabPath);
            var obj = _container.Instantiate(prefab, parent).transform;
            obj.transform.eulerAngles = data.HeadRotation.ToVector3();
            if (data.IsHost) _teacherCharacter = obj;
            else _studentsCharacter[data.Index] = obj;
            _MRTKRig.position = obj.Find("EyeCamPosition").position;
        }

        public async UniTask Spawn() // 24 - 48
        {
            await SpawnTeacherSeat();

            var maxSeat = _userDataController.ServerData.RoomStatus.RoomStatus.MaxAmount;
            await SpawnStudentSeats(maxSeat);

            _studentsCharacter = new Transform[maxSeat];
            await SetupCharacter();
            _classRoom.SetActive(true);
        }

        #endregion Spawn First Enter

        public async void OnJoin(PublicUserData user)
        {
            await SetupCharacter(user);
        }

        public Transform GetCharacterParent(PublicUserData user)
        {
            Transform userSeat = user.IsHost ? _teacherSeatTransform : _seatContainer.GetChild(user.Index);
            Transform parent = userSeat.Find("CharPosition");
            return parent;
        }

        public Transform OnLeave(PublicUserData user = null)
        {
            Transform parent = GetCharacterParent(user);

            foreach (Transform child in parent)
                Object.Destroy(child.gameObject);
            if (!user.IsHost) _studentsCharacter[user.Index] = null;

            return parent;
        }

        #endregion Setup Environment

        public void Clean()
        {
            foreach (Transform child in _studentsCharacter)
                if (child != null) Object.Destroy(child.gameObject);
            if (_teacherCharacter != null) Object.Destroy(_teacherCharacter.gameObject);

            _classRoom.SetActive(false);
            _userDataController.ServerData.RoomStatus = null;
        }

        private void PublishTickData()
        {
            if (_teacherCharacter == null || !_teacherCharacter.gameObject.activeInHierarchy) return;

            Texture2D texture = null;
            if (_userDataController.ServerData.IsSharing)
            {
                RenderTexture.active = _screenRenderTexture;
                texture = new Texture2D(_screenRenderTexture.width, _screenRenderTexture.height);
                texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                texture.Apply();
            }

            _onUserTransformChangePublisher.Publish(new OnVirtualRoomTickSignal(new VirtualRoomTickData
            {
                HeadRotation = _teacherCharacter.eulerAngles.ToVec3D(),
                Texture = _userDataController.ServerData.IsSharing ? null : texture.EncodeToJPG(),
                IsSharing = _userDataController.ServerData.IsSharing,
            }));
        }

        public void Tick()
        {
            if (!_userDataController.ServerData.IsInRoom) return;
            PublishTickData();
        }

        public void OnTransform(PublicUserData user)
        {
            Transform userChar = user.IsHost ? _teacherCharacter : _studentsCharacter[user.Index];
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
            await SetupCharacter(user);
        }

        #region Quizzes

        private QuizzesQuestionView _quizzesQuestionView;
        private QuizzesAnswerView _quizzesAnswerView;

        private PrivateUserData _self => _userDataController.ServerData.RoomStatus.RoomStatus.Self;

        private Transform GetSelfTableUI()
        {
            Transform userSeat = _self.IsHost ? _teacherSeatTransform : _seatContainer.GetChild(_self.Index);
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
