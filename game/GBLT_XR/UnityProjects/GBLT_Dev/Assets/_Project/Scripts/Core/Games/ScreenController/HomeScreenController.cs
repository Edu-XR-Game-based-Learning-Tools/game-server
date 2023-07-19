using Core.Business;
using System.Collections.Generic;
using System.Linq;
using VContainer;

namespace Core.Framework
{
    public class HomeScreenController : IScreenController
    {
        private readonly GameStore.Setting _gameSetting;
        private readonly GameStore _gameStore;
        private readonly IBundleLoader _bundleLoader;

        public ScreenName Name => ScreenName.Home;

        public bool IsAllowChangeScreen(ScreenName newScreen)
        {
            return newScreen != ScreenName.Restart;
        }

        public HomeScreenController(
            GameStore gameStore,
            GameStore.Setting gameSetting,
            IObjectResolver container)
        {
            _gameStore = gameStore;
            _gameSetting = gameSetting;
            _bundleLoader = container.Resolve<IReadOnlyList<IBundleLoader>>().ElementAt((int)BundleLoaderName.Addressable);
        }

        public async void Enter()
        {
            UnityEngine.Debug.Log($"Addressable url: {EnvSetting.AddressableProdUrl}");

            await _gameStore.GetOrCreateModule<IDummy, DummyModel>(
                "", ViewName.Unity, ModuleName.Dummy);
        }

        public void Out()
        {
            return;
        }
    }
}
