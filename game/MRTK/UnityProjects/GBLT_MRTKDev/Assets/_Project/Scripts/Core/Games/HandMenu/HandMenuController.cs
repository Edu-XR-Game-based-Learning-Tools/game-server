using Core.Business;
using Core.EventSignal;
using Core.Extension;
using Core.Framework;
using Core.Module;
using Core.Utility;
using MessagePipe;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using Shared.Network;
using System.Linq;
using TMPro;
using UnityEngine;
using VContainer;
using static UnityEngine.Rendering.DebugUI;

namespace Core.View
{
    public class HandMenuController : MonoBehaviour
    {
        private GameStore _gameStore;
        private ClassRoomHub _classRoomHub;
        private IUserDataController _userDataController;
        private IVoiceCallService _voiceCallService;

        [SerializeField][DebugOnly] private PressableButton _landingPageBtn;
        [SerializeField][DebugOnly] private PressableButton _roomStatusBtn;
        [SerializeField][DebugOnly] private PressableButton _gameStatusBtn;
        [SerializeField][DebugOnly] private PressableButton _openLastScreenBtn;

        [SerializeField][DebugOnly] private PressableButton _isSharingToggle;
        [SerializeField][DebugOnly] private PressableButton _isSharingQuizzesToggle;

        [SerializeField][DebugOnly] private PressableButton _muteToggle;
        [SerializeField][DebugOnly] private PressableButton _handRayToggle;

        [SerializeField][DebugOnly] private bool _isMute = true;
        [SerializeField][DebugOnly] private bool _isToggleHandRay = true;

        private void Awake()
        {
            _landingPageBtn = transform.Find("Canvas/Menu/List").GetChild(0).GetComponent<PressableButton>();
            _roomStatusBtn = transform.Find("Canvas/Menu/List").GetChild(1).GetComponent<PressableButton>();
            _gameStatusBtn = transform.Find("Canvas/Menu/List").GetChild(2).GetComponent<PressableButton>();
            _openLastScreenBtn = transform.Find("Canvas/Menu/List").GetChild(3).GetComponent<PressableButton>();

            _isSharingToggle = transform.Find("Canvas/Menu/Sharing/Toggle List").GetChild(0).GetComponent<PressableButton>();
            _isSharingQuizzesToggle = transform.Find("Canvas/Menu/Sharing/Toggle List").GetChild(1).GetComponent<PressableButton>();

            _muteToggle = transform.Find("Canvas/Menu/Utils/Toggle List").GetChild(0).GetComponent<PressableButton>();
            _handRayToggle = transform.Find("Canvas/Menu/Utils/Toggle List").GetChild(1).GetComponent<PressableButton>();

            RegisterEvents();

            _isSharingToggle.ForceSetToggled(false);
            _isSharingQuizzesToggle.ForceSetToggled(false);
            _handRayToggle.ForceSetToggled(_isToggleHandRay);
        }

        [Inject]
        public void Construct(
            IObjectResolver container
            )
        {
            _gameStore = container.Resolve<GameStore>();
            _userDataController = container.Resolve<IUserDataController>();
            _classRoomHub = container.Resolve<ClassRoomHub>();
            _voiceCallService = container.Resolve<IVoiceCallService>();
        }

        private void RegisterEvents()
        {
            _landingPageBtn.OnClicked.AddListener(async () =>
            {
                _gameStore.RemoveCurrentModel();
                (await _gameStore.GetOrCreateModel<LandingScreen, LandingScreenModel>(
                    moduleName: ModuleName.LandingScreen)).Refresh();
            });
            _roomStatusBtn.OnClicked.AddListener(async () =>
            {
                _gameStore.RemoveCurrentModel();
                (await _gameStore.GetOrCreateModel<RoomStatus, RoomStatusModel>(
                    moduleName: ModuleName.RoomStatus)).Refresh();
            });
            _gameStatusBtn.OnClicked.AddListener(async () =>
            {
                _gameStore.RemoveCurrentModel();
                (await _gameStore.GetOrCreateModel<QuizzesRoomStatus, QuizzesRoomStatusModel>(
                    moduleName: ModuleName.QuizzesRoomStatus)).Refresh();
            });
            //_openLastScreenBtn.OnClicked.AddListener(() =>
            //{
            //    _gameStore.OpenLastHiddenModule();
            //});

            _isSharingToggle.OnClicked.AddListener(() =>
            {
                _userDataController.ServerData.IsSharing = _isSharingToggle.IsToggled;
            });
            _isSharingQuizzesToggle.OnClicked.AddListener(() =>
            {
                _userDataController.ServerData.IsSharingQuizzesGame = _isSharingQuizzesToggle.IsToggled;
            });

            _muteToggle.OnClicked.AddListener(() =>
            {
                Debug.Log($"Mute: {_muteToggle.IsToggled}");
                _isMute = _muteToggle.IsToggled;
                if (_isMute) _voiceCallService.TransmitToNone();
                else _voiceCallService.TransmitToChannel();
            });
            _handRayToggle.OnClicked.AddListener(() =>
            {
                _isToggleHandRay = _handRayToggle.IsToggled;
                SetHandRaysActive(_isToggleHandRay);
            });
            SetHandRaysActive(false);
        }

        private void Update()
        {
            bool isInRoomView = _userDataController.ServerData.IsInRoom;
            bool isInGameView = _userDataController.ServerData.IsInGame;
            _landingPageBtn.SetActive(_gameStore.PassStartSession);
            _roomStatusBtn.SetActive(isInRoomView);
            _gameStatusBtn.SetActive(isInRoomView && isInGameView);
            //_openLastScreenBtn.SetActive(_gameStore.LastHiddenModule != null);

            _isSharingToggle.SetActive(isInRoomView);
            _isSharingQuizzesToggle.SetActive(isInGameView && new QuizzesStatus[] { QuizzesStatus.InProgress, QuizzesStatus.End }.Contains(_userDataController.ServerData.RoomStatus.InGameStatus.JoinQuizzesData.QuizzesStatus));

            _muteToggle.SetActive(isInRoomView);
        }

        /// <summary>
        /// Toggle hand rays.
        /// </summary>
        public void SetHandRaysActive(bool value)
        {
            var handRays = PlayspaceUtilities.XROrigin.GetComponentsInChildren<MRTKRayInteractor>(true);

            foreach (var interactor in handRays)
            {
                interactor.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Toggle gaze pinch interactors.
        /// </summary>
        public void SetGazePinchActive(bool value)
        {
            var gazePinchInteractors = PlayspaceUtilities.XROrigin.GetComponentsInChildren<GazePinchInteractor>(true);

            foreach (var interactor in gazePinchInteractors)
            {
                interactor.gameObject.SetActive(value);
            }
        }
    }
}
