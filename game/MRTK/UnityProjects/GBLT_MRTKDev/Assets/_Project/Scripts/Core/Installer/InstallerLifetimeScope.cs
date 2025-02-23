using Core.Business;
using Core.EventSignal;
using Core.Module;
using Core.Network;
using Core.View;
using MessagePipe;
using Shared.Network;
using System;
using System.Diagnostics;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Unity.Services.Core;
using Unity.Services.Vivox;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;

namespace Core.Framework
{
    public class InstallerLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameRootInstaller _gameRootInstaller = new();
        [SerializeField] private BusinessInstaller _businessInstaller = new();
        [SerializeField] private NetworkInstaller _networkInstaller = new();
        [SerializeField] private UnityServiceInstaller _unityServiceInstaller = new();

        protected override async void Configure(IContainerBuilder builder)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Application.targetFrameRate = 60;

            await _businessInstaller.Init(builder).Install();
            await _networkInstaller.Init(builder).Install();
            await _gameRootInstaller.Init(builder).Install();
            await _unityServiceInstaller.Init(builder).Install();

            UnityEngine.Debug.LogFormat("GameInstaller took {0:0.00} seconds", stopwatch.Elapsed.TotalSeconds);
            stopwatch.Stop();
        }

        private void OnApplicationQuit()
        {
        }
    }

    public interface IInjectInstaller
    {
        IInjectInstaller Init(IContainerBuilder builder);
        UniTask Install();
    }

    [Serializable]
    public class GameRootInstaller : IInjectInstaller
    {
        [SerializeField] private InitialSettingInstaller _initialSettingInstaller;
        public IContainerBuilder builder;

        public IInjectInstaller Init(IContainerBuilder builder)
        { this.builder = builder; return this; }

        public UniTask Install()
        {
            InstallGameModuleState();
            InstallModules();
            InstallServices();
            InstallShareLogger();

            InstallGameSignal();
            InstallUtilities();

            InstallSceneObject();
            InstallScriptableObject();

            InstallEntryPoint();

            return UniTask.CompletedTask;
        }

        private void InstallGameModuleState()
        {
            builder.Register<IScreenController, StartSessionScreenController>(Lifetime.Singleton);
            builder.Register<IScreenController, HomeScreenController>(Lifetime.Singleton);
        }

        private void InstallModules()
        {
            builder.Register<ViewScriptManager>(Lifetime.Singleton);

            builder.Register<IViewContext, UnityViewContext>(Lifetime.Transient);

            // The order of module registration must be the same with ModuleName enum indexes
            builder.Register<IBaseScript, DummyScript>(Lifetime.Scoped);
            builder.Register<IBaseModule, Dummy>(Lifetime.Scoped);

            builder.Register<IBaseScript, SplashScreenScript>(Lifetime.Scoped);
            builder.Register<IBaseModule, SplashScreen>(Lifetime.Scoped);

            builder.Register<IBaseScript, LandingScreenScript>(Lifetime.Scoped);
            builder.Register<IBaseModule, LandingScreen>(Lifetime.Scoped);

            builder.Register<IBaseScript, LoginScreenScript>(Lifetime.Scoped);
            builder.Register<IBaseModule, LoginScreen>(Lifetime.Scoped);

            builder.Register<IBaseScript, ToolDescriptionScript>(Lifetime.Scoped);
            builder.Register<IBaseModule, ToolDescription>(Lifetime.Scoped);

            builder.Register<IBaseScript, SettingScreenScript>(Lifetime.Scoped);
            builder.Register<IBaseModule, SettingScreen>(Lifetime.Scoped);

            builder.Register<IBaseScript, RoomStatusScript>(Lifetime.Scoped);
            builder.Register<IBaseModule, RoomStatus>(Lifetime.Scoped);

            builder.Register<IBaseScript, QuizzesRoomStatusScript>(Lifetime.Scoped);
            builder.Register<IBaseModule, QuizzesRoomStatus>(Lifetime.Scoped);

            // Utils
            builder.Register<IBaseScript, LoadingScript>(Lifetime.Scoped);
            builder.Register<IBaseModule, Loading>(Lifetime.Scoped);

            builder.Register<IBaseScript, PopupScript>(Lifetime.Scoped);
            builder.Register<IBaseModule, Popup>(Lifetime.Scoped);

            builder.Register<IBaseScript, ToastScript>(Lifetime.Scoped);
            builder.Register<IBaseModule, Toast>(Lifetime.Scoped);

            builder.Register<BaseModule.Factory>(Lifetime.Singleton);
            builder.Register<BaseViewContext.Factory>(Lifetime.Singleton);
            builder.Register<BaseViewScript.Factory>(Lifetime.Singleton);
        }

        private void InstallServices()
        {
            builder.Register<IBundleLoader, ResourceLoader>(Lifetime.Singleton);
            builder.Register<IBundleLoader, AddressableLoader>(Lifetime.Singleton);
        }

        private void InstallShareLogger()
        {
            builder.Register<Business.ILogger, UnityDebugLogger>(Lifetime.Singleton);
            builder.Register<ErrorHandler>(Lifetime.Singleton);
        }

        private void InstallGameSignal()
        {
            var options = builder.RegisterMessagePipe();
            builder.RegisterBuildCallback(c => GlobalMessagePipe.SetProvider(c.AsServiceProvider()));

            builder.RegisterMessageBroker<GameActionSignal<IModuleContextModel>>(options);
            builder.RegisterMessageBroker<OnNetworkRetryExceedMaxRetriesSignal>(options);
            builder.RegisterMessageBroker<UserDataCachedSignal>(options);
            builder.RegisterMessageBroker<OnVirtualRoomTickSignal>(options);

            builder.RegisterMessageBroker<GameScreenChangeSignal>(options);
            builder.RegisterMessageBroker<GameScreenForceChangeSignal>(options);
            builder.RegisterMessageBroker<ShowToastSignal>(options);
            builder.RegisterMessageBroker<ShowLoadingSignal>(options);
            builder.RegisterMessageBroker<ShowPopupSignal>(options);

            builder.RegisterMessageBroker<CheckDownloadSizeStatusSignal>(options);
            builder.RegisterMessageBroker<UpdateLoadingProgressSignal>(options);
            builder.RegisterMessageBroker<AddressableErrorSignal>(options);

            builder.RegisterMessageBroker<GameAudioSignal>(options);
            builder.RegisterMessageBroker<PlayOneShotAudioSignal>(options);

            builder.RegisterMessageBroker<OnApplicationQuitSignal>(options);
        }

        private void InstallUtilities()
        {
            builder.Register<JsonFileReader>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<AtlasManager>(Lifetime.Singleton);
        }

        private void InstallSceneObject()
        {
            builder.RegisterComponentInHierarchy<ViewLayerManager>().As<IViewLayerManager>();
            builder.RegisterComponentInHierarchy<UTKitViewLayerManager>().As<IViewLayerManager>();
            builder.RegisterComponentInHierarchy<MRTK3ViewLayerManager>().As<IViewLayerManager>();

            builder.RegisterComponentInHierarchy<PoolObjectMono>();
            builder.RegisterComponentInHierarchy<AudioController>();
            builder.RegisterComponentInHierarchy<HandMenuController>();

            builder.RegisterComponentInHierarchy<VirtualRoomPresenter>();
        }

        private void InstallScriptableObject()
        {
            builder.RegisterInstance(_initialSettingInstaller.GameSetting);
            builder.RegisterInstance(_initialSettingInstaller.Atlas);
            builder.RegisterInstance(_initialSettingInstaller.NetworkSettings);
        }

        private void InstallEntryPoint()
        {
            builder.RegisterEntryPoint<GameStore>(Lifetime.Singleton).AsSelf();
        }
    }

    [Serializable]
    public class BusinessInstaller : IInjectInstaller
    {
        public IContainerBuilder builder;

        public IInjectInstaller Init(IContainerBuilder builder)
        { this.builder = builder; return this; }

        public UniTask Install()
        {
            builder.Register<BasePoolObject.Factory>(Lifetime.Singleton);
            builder.Register<IPoolManager, PoolManager>(Lifetime.Singleton);
            builder.Register<IPoolManager, AudioPoolManager>(Lifetime.Singleton);

            builder.Register<PlayerPrefManager>(Lifetime.Singleton);
            builder.Register<IDefinitionManager, DefinitionManager>(Lifetime.Singleton);

            //builder.Register<IDefinitionLoader, DefinitionLoader>(Lifetime.Singleton);
            builder.Register<IDefinitionLoader, RemoteDefinitionLoader>(Lifetime.Singleton);

#if UNITY_EDITOR
            //builder.Register<IDefinitionLoader, DefinitionLoader>(Lifetime.Singleton);
#endif

            return UniTask.CompletedTask;
        }
    }

    [Serializable]
    public class NetworkInstaller : IInjectInstaller
    {
        public IContainerBuilder builder;

        public IInjectInstaller Init(IContainerBuilder builder)
        { this.builder = builder; return this; }

        public UniTask Install()
        {
            builder.Register<IDefinitionDataController, DefinitionDataController>(Lifetime.Singleton);

            builder.Register<EndPointSwitcher>(Lifetime.Singleton);
            builder.Register<IGRpcHubClient, GRpcHubClient>(Lifetime.Transient);

            builder.Register<IUserDataController, UserDataController>(Lifetime.Singleton);

            builder.Register<IGRpcServiceClient, GRpcServiceClient>(Lifetime.Singleton);
            builder.Register<IRpcAuthController, AuthRestController>(Lifetime.Singleton);
            builder.Register<IDataServiceController, DataRestController>(Lifetime.Singleton);

            //builder.Register<IVoiceCallService, VivoxVoiceCallService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<VivoxVoiceCallService>().As<IVoiceCallService>().AsSelf();

            builder.Register<GRpcAuthenticationFilter>(Lifetime.Singleton);
            builder.Register<GRpcRetryHandlerFilter>(Lifetime.Singleton);

            builder.Register<ClassRoomHub>(Lifetime.Singleton);
            builder.Register<QuizzesHub>(Lifetime.Singleton);

            builder.Register<UserAuthentication>(Lifetime.Singleton);

            return UniTask.CompletedTask;
        }
    }

    [Serializable]
    public class UnityServiceInstaller : IInjectInstaller
    {
        public IInjectInstaller Init(IContainerBuilder _)
        { return this; }

        [SerializeField] private string _key;
        [SerializeField] private string _issuer;
        [SerializeField] private string _domain;
        [SerializeField] private string _server;

        private bool CheckManualCredentials()
        {
            return !(string.IsNullOrEmpty(_issuer) && string.IsNullOrEmpty(_domain) && string.IsNullOrEmpty(_server));
        }

        public async UniTask Install()
        {
            var options = new InitializationOptions();
            if (CheckManualCredentials())
            {
                options.SetVivoxCredentials(_server, _domain, _issuer, _key);
            }

            await UnityServices.InitializeAsync(options);
            if (!CheckManualCredentials())
            {
                AuthenticationService.Instance.ClearSessionToken();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            await VivoxService.Instance.InitializeAsync();
        }
    }
}
