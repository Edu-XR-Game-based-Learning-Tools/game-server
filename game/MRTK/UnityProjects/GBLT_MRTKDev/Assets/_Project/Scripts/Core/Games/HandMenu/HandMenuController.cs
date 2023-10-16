using Core.Business;
using Core.EventSignal;
using Core.Extension;
using Core.Framework;
using Core.Module;
using Core.Utility;
using MessagePipe;
using Microsoft.MixedReality.Toolkit.UX;
using Shared.Network;
using TMPro;
using UnityEngine;
using VContainer;

namespace Core.View
{
    public class HandMenuController : MonoBehaviour
    {
        private GameStore _gameStore;
        private ClassRoomHub _classRoomHub;
        private IUserDataController _userDataController;

        [SerializeField][DebugOnly] private PressableButton _landingPageBtn;
        [SerializeField][DebugOnly] private PressableButton _roomStatusBtn;
        [SerializeField][DebugOnly] private PressableButton _gameStatusBtn;
        [SerializeField][DebugOnly] private PressableButton _openLastScreenBtn;
        [SerializeField][DebugOnly] private PressableButton _shareBtn;

        private void Awake()
        {
            _landingPageBtn = transform.Find("Canvas/Menu/List").GetChild(0).GetComponent<PressableButton>();
            _roomStatusBtn = transform.Find("Canvas/Menu/List").GetChild(1).GetComponent<PressableButton>();
            _gameStatusBtn = transform.Find("Canvas/Menu/List").GetChild(2).GetComponent<PressableButton>();
            _openLastScreenBtn = transform.Find("Canvas/Menu/List").GetChild(3).GetComponent<PressableButton>();
            _shareBtn = transform.Find("Canvas/Menu/List").GetChild(4).GetComponent<PressableButton>();

            RegisterEvents();
        }

        [Inject]
        public void Construct(
            IObjectResolver container
            )
        {
            _gameStore = container.Resolve<GameStore>();
            _userDataController = container.Resolve<IUserDataController>();
        }

        private void RegisterEvents()
        {
            _landingPageBtn.OnClicked.AddListener(async () =>
            {
                _gameStore.RemoveCurrentModel();
                (await _gameStore.GetOrCreateModule<LandingScreen, LandingScreenModel>(
                    "", ViewName.Unity, ModuleName.RoomStatus)).Model.Refresh();
            });
            _roomStatusBtn.OnClicked.AddListener(async () =>
            {
                _gameStore.RemoveCurrentModel();
                (await _gameStore.GetOrCreateModule<RoomStatus, RoomStatusModel>(
                    "", ViewName.Unity, ModuleName.RoomStatus)).Model.Refresh();
            });
            _gameStatusBtn.OnClicked.AddListener(async () =>
            {
                _gameStore.RemoveCurrentModel();
                (await _gameStore.GetOrCreateModule<QuizzesRoomStatus, QuizzesRoomStatusModel>(
                    "", ViewName.Unity, ModuleName.QuizzesRoomStatus)).Model.Refresh();
            });
            _openLastScreenBtn.OnClicked.AddListener(() =>
            {
                _gameStore.OpenLastHiddenModule();
            });

            _shareBtn.OnClicked.AddListener(() =>
            {
                _userDataController.ServerData.IsSharing = !_userDataController.ServerData.IsSharing;
                _shareBtn.transform.Find("Frontplate/AnimatedContent/Text").GetComponent<TextMeshProUGUI>().text = _userDataController.ServerData.IsSharing ? "Stop sharing" : "Share";
            });
        }

        private void Update()
        {
            bool isInRoomView = _userDataController.ServerData.IsInRoom;
            bool isInGameView = _userDataController.ServerData.IsInGame;
            _roomStatusBtn.SetActive(isInRoomView);
            _gameStatusBtn.SetActive(isInRoomView && isInGameView);
            _openLastScreenBtn.SetActive(_gameStore.LastHiddenModule != null);
        }
    }
}
