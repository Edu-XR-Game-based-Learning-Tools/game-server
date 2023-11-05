using Core.Business;
using Core.Module;
using Core.Network;
using Cysharp.Threading.Tasks;
using MagicOnion;
using Shared.Network;
using System.Collections.Generic;
using System.Linq;
using VContainer;

namespace Core.Framework
{
    public class StartSessionScreenController : IScreenController
    {
        private readonly GameStore.Setting _gameSetting;
        private readonly GameStore _gameStore;
        private readonly IBundleLoader _bundleLoader;
        private readonly IDefinitionLoader _definitionLoader;
        private readonly IDataServiceController _dataServiceController;
        private readonly IDefinitionDataController _definitionDataController;
        private readonly VirtualRoomPresenter _virtualRoomPresenter;
        private readonly ClassRoomHub _classRoomHub;

        public ScreenName Name => ScreenName.SessionStart;

        public bool IsAllowChangeScreen(ScreenName newScreen)
        {
            return newScreen != ScreenName.Restart;
        }

        public StartSessionScreenController(
            GameStore gameStore,
            GameStore.Setting gameSetting,
            IObjectResolver container)
        {
            _gameStore = gameStore;
            _gameSetting = gameSetting;
            _bundleLoader = container.Resolve<IReadOnlyList<IBundleLoader>>().ElementAt((int)BundleLoaderName.Addressable);
            _definitionLoader = container.Resolve<IDefinitionLoader>();
            _dataServiceController = container.Resolve<IDataServiceController>();
            _definitionDataController = container.Resolve<IDefinitionDataController>();
            _virtualRoomPresenter = container.Resolve<VirtualRoomPresenter>();
            _classRoomHub = container.Resolve<ClassRoomHub>();
        }

        public async void Enter()
        {
            UnityEngine.Debug.Log($"Addressable url: {EnvSetting.AddressableProdUrl}");
            byte[] definitions = await _dataServiceController.LoadDefinitions();
            ((RemoteDefinitionLoader)_definitionLoader).InitMemoryDefinitions(definitions);

            await _definitionDataController.VerifyClient();
            _virtualRoomPresenter.Init();
            _classRoomHub.Init();

            //await _gameStore.GetOrCreateModule<IDummy, DummyModel>(
            //    "", ViewName.Unity, ModuleName.Dummy);

            await _gameStore.GetOrCreateModule<SplashScreen, SplashScreenModel>(
                "", ViewName.Unity, ModuleName.SplashScreen);

            await _gameStore.GetOrCreateModule<Popup, PopupModel>(
                "", ViewName.Unity, ModuleName.Popup);
            await _gameStore.GetOrCreateModule<Toast, ToastModel>(
                "", ViewName.Unity, ModuleName.Toast);
            await _gameStore.GetOrCreateModule<Loading, LoadingModel>(
                "", ViewName.Unity, ModuleName.Loading);
        }

        public void Out()
        {
            return;
        }
    }
}
