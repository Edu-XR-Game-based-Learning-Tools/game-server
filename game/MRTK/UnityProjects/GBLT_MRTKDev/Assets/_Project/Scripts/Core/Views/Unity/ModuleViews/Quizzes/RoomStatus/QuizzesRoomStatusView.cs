using Core.Business;
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
using UnityEngine.UI;
using VContainer;

namespace Core.View
{
    [System.Serializable]
    public class QuizzesRoomStatusPerson
    {
        [DebugOnly] public PressableButton Button;
        [DebugOnly] public TextMeshProUGUI NameTxt;
        [DebugOnly] public Image IconImg;
    }

    public class QuizzesRoomStatusView : UnityView
    {
        private GameStore _gameStore;
        private AudioPoolManager _audioPoolManager;
        private VirtualRoomPresenter _virtualRoomPresenter;
        private IUserDataController _userDataController;

        [SerializeField][DebugOnly] private PressableButton _closeBtn;
        [SerializeField][DebugOnly] private PressableButton _backBtn;

        [SerializeField][DebugOnly] private PressableButton _startBtn;
        [SerializeField][DebugOnly] private TextMeshProUGUI _titleTxt;
        [SerializeField][DebugOnly] private TextMeshProUGUI _amountTxt;

        [SerializeField][DebugOnly] private QuizzesRoomStatusPerson[] _personItems;

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
            _closeBtn = transform.Find("CanvasDialog/Canvas/Header/RightSide/Close_Btn").GetComponent<PressableButton>();
            _backBtn = transform.Find("CanvasDialog/Canvas/Header/Back_Btn").GetComponent<PressableButton>();

            _startBtn = transform.Find("CanvasDialog/Canvas/Header/Start_Btn").GetComponent<PressableButton>();
            _titleTxt = transform.Find("CanvasDialog/Canvas/Header/Content/Title").GetComponent<TextMeshProUGUI>();
            _amountTxt = transform.Find("CanvasDialog/Canvas/Header/Content/Amount").GetComponent<TextMeshProUGUI>();

            var list = transform.Find("CanvasDialog/Canvas/Content/Scroll View/Viewport/Content");
            _personItems = new bool[list.childCount].Select((_, idx) =>
            {
                Transform person = list.GetChild(idx);
                return new QuizzesRoomStatusPerson
                {
                    Button = person.GetComponent<PressableButton>(),
                    NameTxt = person.Find("Frontplate/AnimatedContent/Text").GetComponent<TextMeshProUGUI>(),
                    IconImg = person.Find("Frontplate/AnimatedContent/Icon/UIButtonSpriteIcon").GetComponent<Image>(),
                };
            }).ToArray();
        }

        private void RegisterEvents()
        {
            _backBtn.OnClicked.AddListener(async () =>
            {
                _gameStore.GState.RemoveModel<QuizzesRoomStatusModel>();
                await _gameStore.GetOrCreateModule<LandingScreen, LandingScreenModel>(
                    "", ViewName.Unity, ModuleName.LandingScreen);
            });

            _startBtn.OnClicked.AddListener(async () =>
            {
                _gameStore.GState.RemoveModel<QuizzesRoomStatusModel>();
                await _virtualRoomPresenter.Spawn();
            });

            for (int idx = 0; idx < _personItems.Length; idx++)
            {
                int index = idx;
                _personItems[index].Button.OnClicked.AddListener(() =>
                {
                    Debug.Log($"{_personItems[index].NameTxt.text} - {index}");
                });
            }
        }

        public override void OnReady()
        {
            GetReferences();
            RegisterEvents();

            Refresh();
        }

        public void Refresh()
        {
            if (!_userDataController.ServerData.IsInRoom)
            {
                _gameStore.GState.RemoveModel<QuizzesRoomStatusModel>();
                return;
            }

            bool isInGameView = _userDataController.ServerData.IsInGame;
            GeneralRoomStatusResponse status = isInGameView ? _userDataController.ServerData.RoomStatus.InGameStatus : _userDataController.ServerData.RoomStatus.RoomStatus;

            string idPrefix = isInGameView ? "PIN" : "Room Id";
            _titleTxt.SetText($"{idPrefix}: {status.Id}");
            _amountTxt.SetText($"Amount: {status.Amount}");
            for (int idx = 0; idx < _personItems.Length; idx++)
            {
                _personItems[idx].Button.SetActive(idx >= status.Others.Length);
                if (idx >= status.Others.Length) continue;

                if (status.Others[idx].IsHost)
                {
                    idx--;
                    continue;
                }

                _personItems[idx].NameTxt.text = status.Others[idx].Name;
            }
            _startBtn.SetActive(isInGameView);
        }
    }
}
