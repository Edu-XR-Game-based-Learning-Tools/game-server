using Core.Business;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using VContainer;

namespace Core.Module
{
    public abstract class BaseModule : IBaseModule
    {
        public class Factory
        {
            private readonly IObjectResolver _container;

            public Factory(IObjectResolver objectResolver)
            {
                _container = objectResolver;
            }

            public IBaseModule Create(ModuleName moduleName)
            {
                IReadOnlyList<IBaseModule> modules = _container.Resolve<IReadOnlyList<IBaseModule>>();
                IBaseModule module = modules.ElementAt((int)moduleName);
                return module;
            }
        }

        public IViewContext ViewContext { get; private set; }
        public ModuleName ModuleName { get; private set; }

        protected abstract void OnViewReady();

        protected abstract void OnDisposed();

        public virtual UniTask Initialize()
        { return UniTask.FromResult(0); }

        public virtual async UniTask CreateView(
            string viewId,
            ModuleName moduleName,
            IViewContext viewContext)
        {
            ModuleName = moduleName;
            ViewContext = viewContext;
            await ViewContext.TryCreateViewElement(this);
            OnViewReady();
            ViewContext.OnReady();
        }

        public virtual void Remove()
        {
            if (ViewContext != null)
            {
                ViewContext.Destroy();
                OnDisposed();
            }
        }

        public abstract void Refresh(IModuleContextModel model);
    }
}
