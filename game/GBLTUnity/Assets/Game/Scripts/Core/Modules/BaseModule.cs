using Core.Business;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Core.Module
{
    public abstract class BaseModule : IBaseModule
    {
        public class Factory : PlaceholderFactory<IBaseModule>
        { }

        protected IViewContext _viewContext { get; set; }

        protected abstract void OnViewReady();

        protected abstract void OnDisposed();

        public virtual UniTask Initialize()
        { return UniTask.FromResult(0); }

        public virtual async UniTask CreateView(
            string viewId,
            ViewName viewName,
            IViewCustomFactory viewContextFactory)
        {
            _viewContext = await viewContextFactory.Create(this, viewId, viewName);
            OnViewReady();
            _viewContext.OnReady();
        }

        public virtual void Remove()
        {
            if (_viewContext != null)
            {
                _viewContext.Destroy();
                OnDisposed();
            }
        }

        public virtual async void RemoveAfter(float seconds)
        {
            if (_viewContext == null)
                return;

            await UniTask.Delay((int)(seconds * 1000));
            Remove();
        }

        public abstract void Refresh(IModuleContextModel model);

        public abstract void CustomRefresh(IModuleContextModel model, string comparer);

        public virtual void AddSubView(IViewContext subview)
        { }
    }
}