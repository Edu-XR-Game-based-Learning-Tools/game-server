using Core.Business;
using Core.Extension;
using Core.Framework;
using Core.Module;
using Core.Utility;
using Microsoft.MixedReality.Toolkit.UX;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

namespace Core.View
{
    public enum SettingScreenTabType
    {
        General,
        Video,
        Audio,
        Chat,
    }

    public class SettingScreenView : UnityView
    {
        private GameStore _gameStore;
        private AudioPoolManager _audioPoolManager;

        [SerializeField][DebugOnly] private PressableButton _backBtn;
        [SerializeField][DebugOnly] private PressableButton[] _tabBtns;
        [SerializeField][DebugOnly] private Transform[] _tabContents;

        [SerializeField][DebugOnly] private int _tabActiveIndex;

        [Inject]
        public void Init(
            GameStore gameStore,
            IObjectResolver container)
        {
            _gameStore = gameStore;
            _audioPoolManager = (AudioPoolManager)container.Resolve<IReadOnlyList<IPoolManager>>().ElementAt((int)PoolName.Audio);
        }

        private void GetReferences()
        {
            _backBtn = transform.Find("CanvasDialog/Canvas/Header/Back_Btn").GetComponent<PressableButton>();
            var tab = transform.Find("CanvasDialog/Canvas/Content/Tabs");
            _tabBtns = new bool[tab.childCount].Select((_, idx) => tab.GetChild(idx).GetComponent<PressableButton>()).ToArray();
            var tabContent = transform.Find("CanvasDialog/Canvas/Content/TabContents");
            _tabContents = new bool[tabContent.childCount].Select((_, idx) => tabContent.GetChild(idx)).ToArray();
        }

        private void SetTabActive(int index, bool isActive)
        {
            _tabContents[index].SetActive(isActive);
            _tabBtns[index].transform.Find("Backplate_Active").SetActive(isActive);
            if (isActive) _tabActiveIndex = index;
        }

        private void RegisterEvents()
        {
            _backBtn.OnClicked.AddListener(async () =>
            {
                _gameStore.GState.RemoveModel<SettingScreenModel>();
                await _gameStore.GetOrCreateModule<LandingScreen, LandingScreenModel>(
                    "", ViewName.Unity, ModuleName.LandingScreen);
            });

            for (int idx = 0; idx < _tabBtns.Length; idx++)
            {
                int index = idx;
                _tabBtns[index].OnClicked.AddListener(() =>
                {
                    SetTabActive(_tabActiveIndex, false);
                    SetTabActive(index, true);
                });
            }
        }

        public override void OnReady()
        {
            GetReferences();
            RegisterEvents();

            SetTabActive(0, true);
        }

        public void Refresh()
        {
        }
    }
}
