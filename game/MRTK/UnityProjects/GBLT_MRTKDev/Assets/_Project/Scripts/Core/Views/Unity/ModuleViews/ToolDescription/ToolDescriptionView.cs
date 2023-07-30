using Core.Business;
using Core.Framework;
using Core.Module;
using Core.Utility;
using Microsoft.MixedReality.Toolkit.UX;
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
        }

        private void GetReferences()
        {
            _backBtn = transform.Find("CanvasDialog/Canvas/Header/Back_Btn").GetComponent<PressableButton>();
            _videoPlayer = transform.Find("CanvasDialog/Canvas/Content/Content/Video").GetComponent<VideoPlayer>();
            _titleTxt = transform.Find("CanvasDialog/Canvas/Content/Content/Content/Title").GetComponent<TextMeshProUGUI>();
            _descriptionTxt = transform.Find("CanvasDialog/Canvas/Content/Content/Content/Description").GetComponent<TextMeshProUGUI>();
            _timeTxt = transform.Find("CanvasDialog/Canvas/Footer/Left/AnimatedContent/Text").GetComponent<TextMeshProUGUI>();
            _openBtn = transform.Find("CanvasDialog/Canvas/Footer/Right/Open_Btn").GetComponent<PressableButton>();
        }

        private void RegisterEvents()
        {
            _backBtn.OnClicked.AddListener(async () =>
            {
                _gameStore.GState.RemoveModel<ToolDescriptionModel>();
                await _gameStore.GetOrCreateModule<LandingScreen, LandingScreenModel>(
                    "", ViewName.Unity, ModuleName.LandingScreen);
            });

            _openBtn.OnClicked.AddListener(() =>
            {
                Debug.Log(CoreDefines.NotAvailable);
            });
        }

        public override void OnReady()
        {
            GetReferences();
            RegisterEvents();
        }

        public void Refresh()
        {
        }
    }
}
