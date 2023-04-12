using Core.Business;
using Core.EventSignal;
using Cysharp.Threading.Tasks;
using System;
using Zenject;

namespace Core.Framework
{
    public partial class GameStore : IInitializable, IDisposable
    {
        private readonly Setting _gameSetting;
        private readonly SignalBus _signalBus;
        private readonly DiContainer _container;
        private readonly IViewCustomFactory _viewContextFactory;

        public Setting GameSetting => _gameSetting;

        private Action<GameAction, IModuleContextModel> Reducers;
        public ModelState GState { get; set; }
        private IScreenController _currentScreen;
        public ScreenName CurrentScreenName => _currentScreen.Name;

        public GameStore(
            Setting gameSetting,
            SignalBus signalBus,
            DiContainer container,
            IViewCustomFactory viewContextFactory)
        {
            _gameSetting = gameSetting;
            _signalBus = signalBus;
            _container = container;
            _viewContextFactory = viewContextFactory;

            ReducerDelegating();
            GState = new ModelState();
        }

        public bool ChangeScreen(ScreenName newScreenName, bool isOutOldScreen = true)
        {
            if (newScreenName == _currentScreen.Name || !_currentScreen.IsAllowChangeScreen(newScreenName))
                return false;

            if (isOutOldScreen) _currentScreen.Out();
            EnterNewScreen(newScreenName);
            return true;
        }

        public bool ForceChangeScreen(ScreenName newScreenName)
        {
            _currentScreen.Out();
            EnterNewScreen(newScreenName);
            ScreenName previousScreenName = _currentScreen != null ? _currentScreen.Name : ScreenName.SessionStart;
            _signalBus.Fire(new GameScreenForceChangeSignal(newScreenName, previousScreenName));
            return true;
        }

        public void ReEnterScreen()
        {
            EnterNewScreen(CurrentScreenName);
        }

        private void EnterNewScreen(ScreenName newScreenName)
        {
            ScreenName previousScreenName = _currentScreen != null ? _currentScreen.Name : ScreenName.SessionStart;
            _currentScreen = _container.ResolveId<IScreenController>(newScreenName);
            _currentScreen.Enter();
            if (previousScreenName != newScreenName)
                _signalBus.Fire(new GameScreenChangeSignal(newScreenName, previousScreenName));
        }

        public void Initialize()
        {
            _signalBus.Subscribe<GameActionSignal<IModuleContextModel>>(Dispatch);

            EnterNewScreen(ScreenName.SessionStart);
        }

        #region Reducer pattern

        private void ReducerDelegating()
        {
            Reducers += TransitioningHandler;
        }

        private void Dispatch(GameActionSignal<IModuleContextModel> signal)
        {
            if (Reducers != null)
                Reducers(signal.Action, signal.NewModel);
        }

        private void TransitioningHandler(GameAction action, IModuleContextModel model)
        {
            // test keep this structure to see in future if needed.
        }

        #endregion Reducer pattern

        public async UniTask<TClass> CreateModule<TClass, TModel>(
            string viewId,
            ViewName viewName,
            ModuleName moduleName)
            where TClass : IBaseModule
            where TModel : IModuleContextModel, new()
        {
            if (GState.HasModel<TModel>())
            {
                //_logger.Warning($"Dupplicate Found on Module: {typeof(TClass).ToString()}");
                return (TClass)GState.GetModel<TModel>().Module;
            }
            if (viewName == ViewName.Unity) viewId = moduleName.ToString() + "Script";
            TClass module = await CreateModuleInner<TClass, TModel>(viewId, viewName, moduleName);
            CreateModel<TClass, TModel>(module);
            await module.CreateView(viewId, viewName, _viewContextFactory);
            return module;
        }

        private async UniTask<TClass> CreateModuleInner<TClass, TModel>(
            string viewId,
            ViewName viewName,
            ModuleName moduleName)
            where TClass : IBaseModule
            where TModel : IModuleContextModel, new()
        {
            Module.BaseModule.Factory baseContextFactory = _container.ResolveId<Module.BaseModule.Factory>(moduleName);
            TClass instance = (TClass)baseContextFactory.Create();
            await instance.Initialize();
            return instance;
        }

        private void CreateModel<TClass, TModel>(TClass module)
            where TClass : IBaseModule
            where TModel : IModuleContextModel, new()
        {
            TModel model = GState.CreateNewModel<TModel>();
            model.Module = module;
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<GameActionSignal<IModuleContextModel>>(Dispatch);
        }

        public void RemoveAllModules()
        {
            GState.RemoveAllModules();
        }
    }
}