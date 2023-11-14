using Core.Business;
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
    public class SplashScreenView : UnityView
    {
        private GameStore _gameStore;
        private AudioPoolManager _audioPoolManager;

        [SerializeField][DebugOnly] private PressableButton _startBtn;

        [Inject]
        public void Init(
            GameStore gameStore,
            IObjectResolver container)
        {
            _gameStore = gameStore;
            _audioPoolManager = (AudioPoolManager)container.Resolve<IReadOnlyList<IPoolManager>>().ElementAt((int)PoolName.Audio);
        }

        private void RegisterEvents()
        {
            _startBtn.OnClicked.AddListener(async () =>
            {
                _gameStore.GState.RemoveModel<SplashScreenModel>();
                (await _gameStore.GetOrCreateModel<LandingScreen, LandingScreenModel>(
                    moduleName: ModuleName.LandingScreen)).Refresh();
            });
        }

        public override void OnReady()
        {
            _startBtn = transform.Find("CanvasDialog/Canvas/Horizontal/Start_Btn").GetComponent<PressableButton>();

            RegisterEvents();
        }

        public void Refresh()
        {
        }
    }
}
