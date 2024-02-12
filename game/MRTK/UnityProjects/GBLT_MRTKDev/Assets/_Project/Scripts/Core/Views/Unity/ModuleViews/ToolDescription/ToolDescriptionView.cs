using Core.Business;
using Core.EventSignal;
using Core.Extension;
using Core.Framework;
using Core.Module;
using Core.Utility;
using Microsoft.MixedReality.Toolkit.UX;
using Shared.Network;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using VContainer;

namespace Core.View
{
    public class ToolDescriptionView : UnityView
    {
        private GameStore _gameStore;
        private AudioPoolManager _audioPoolManager;
        private VirtualRoomPresenter _virtualRoomPresenter;
        private ClassRoomHub _classRoomHub;
        private QuizzesHub _quizzesHub;
        private IUserDataController _userDataController;

        [SerializeField][DebugOnly] private PressableButton _closeBtn;
        [SerializeField][DebugOnly] private PressableButton _backBtn;

        [SerializeField][DebugOnly] private VideoPlayer _videoPlayer;
        [SerializeField][DebugOnly] private TextMeshProUGUI _titleTxt;
        [SerializeField][DebugOnly] private TextMeshProUGUI _descriptionTxt;
        [SerializeField][DebugOnly] private TextMeshProUGUI _timeTxt;
        [SerializeField][DebugOnly] private PressableButton _openBtn;

        [Inject]
        public void Init(
            GameStore gameStore,
            IObjectResolver container)
        {
            _gameStore = gameStore;
            _audioPoolManager = (AudioPoolManager)container.Resolve<IReadOnlyList<IPoolManager>>().ElementAt((int)PoolName.Audio);
            _virtualRoomPresenter = container.Resolve<VirtualRoomPresenter>();
            _classRoomHub = container.Resolve<ClassRoomHub>();
            _quizzesHub = container.Resolve<QuizzesHub>();
            _userDataController = container.Resolve<IUserDataController>();
        }

        private void GetReferences()
        {
            _closeBtn = transform.Find("CanvasDialog/Canvas/Header/Close_Btn").GetComponent<PressableButton>();
            _backBtn = transform.Find("CanvasDialog/Canvas/Header/Back_Btn").GetComponent<PressableButton>();

            _videoPlayer = transform.Find("CanvasDialog/Canvas/Content/Content/Video").GetComponent<VideoPlayer>();
            _titleTxt = transform.Find("CanvasDialog/Canvas/Content/Content/Content/Title").GetComponent<TextMeshProUGUI>();
            _descriptionTxt = transform.Find("CanvasDialog/Canvas/Content/Content/Content/Description").GetComponent<TextMeshProUGUI>();
            _timeTxt = transform.Find("CanvasDialog/Canvas/Footer/Left/AnimatedContent/Text").GetComponent<TextMeshProUGUI>();
            _openBtn = transform.Find("CanvasDialog/Canvas/Footer/Right/Open_Btn").GetComponent<PressableButton>();
        }

        private void RegisterEvents()
        {
            _closeBtn.OnClicked.AddListener(() =>
            {
                _gameStore.HideCurrentModule(ModuleName.ToolDescription);
            });
            _backBtn.OnClicked.AddListener(async () =>
            {
                _gameStore.GState.RemoveModel<ToolDescriptionModel>();
                (await _gameStore.GetOrCreateModel<LandingScreen, LandingScreenModel>(
                    moduleName: ModuleName.LandingScreen)).Refresh();
            });

            _openBtn.OnClicked.AddListener(async () =>
            {
                _showLoadingPublisher.Publish(new ShowLoadingSignal());

                QuizzesStatusResponse response = await _quizzesHub.JoinAsync(new JoinQuizzesData(), true);
                if (_gameStore.CheckShowToastIfNotSuccessNetwork(response))
                    return;

                _userDataController.ServerData.RoomStatus.InGameStatus = response;
                await _virtualRoomPresenter.OnSelfJoinQuizzes();
                await _classRoomHub.InviteToGame(response);

                _gameStore.GState.RemoveModel<LandingScreenModel>();
                await _gameStore.GetOrCreateModel<QuizzesRoomStatus, QuizzesRoomStatusModel>(
                    moduleName: ModuleName.QuizzesRoomStatus);

                _showLoadingPublisher.Publish(new ShowLoadingSignal(isShow: false));
            });
        }

        public override void OnReady()
        {
            GetReferences();
            RegisterEvents();

            Refresh();
        }

        public void Refresh()
        {
            bool isInRoomView = _userDataController.ServerData.IsInRoom;
            bool isInGameView = _userDataController.ServerData.IsInGame;
            _openBtn.SetActive(false && isInRoomView && !isInGameView && _userDataController.ServerData.RoomStatus.RoomStatus.Self.IsHost);
        }
    }
}
