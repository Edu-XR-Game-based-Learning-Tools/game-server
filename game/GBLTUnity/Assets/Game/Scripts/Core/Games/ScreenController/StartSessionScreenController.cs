using Core.Business;
using Core.EventSignal;

using Cysharp.Threading.Tasks;

using System.Collections.Generic;

using Zenject;

namespace Core.Framework
{
    public class StartSessionScreenController : IScreenController
    {
        private readonly ILogger _logger;
        private readonly GameStore.Setting _gameSetting;
        private readonly GameStore _gameStore;
        private readonly IBundleLoader _bundleLoader;
        private readonly SignalBus _signalBus;

        public ScreenName Name => ScreenName.SessionStart;

        public bool IsAllowChangeScreen(ScreenName newScreen)
        {
            return newScreen != ScreenName.Restart;
        }

        public StartSessionScreenController(
            ILogger logger,
            SignalBus signalBus,
            [Inject(Id = BundleLoaderName.Addressable)]
            IBundleLoader bundleLoader,
            GameStore gameStore,
            GameStore.Setting gameSetting)
        {
            _logger = logger;
            _bundleLoader = bundleLoader;
            _gameStore = gameStore;
            _gameSetting = gameSetting;
            _signalBus = signalBus;
        }

        public async void Enter()
        {
            _logger.Log($"Addressable url: {EnvSetting.AddressableProdUrl}");

            await _gameStore.CreateModule<IDummyXR, DummyXRModel>(
                "", ViewName.Unity, ModuleName.DummyXR);
        }

        public void Out()
        {
            return;
        }
    }
}