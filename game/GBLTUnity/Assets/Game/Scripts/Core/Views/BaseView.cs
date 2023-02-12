using Core.Business;
using Cysharp.Threading.Tasks;
using System;
using Zenject;

namespace Core.View
{
    public abstract class BaseViewContext : IViewContext
    {
        public class Factory : PlaceholderFactory<string, IViewContext>
        { }

        public BaseViewConfig ConfigModel { get; set; }

        public abstract UnityEngine.GameObject View { get; }
        public IBaseModule Module { get; set; }

        public abstract UniTask TryCreateViewElement(IBaseModule module);

        public abstract void Call<T>(T function, params object[] args) where T : Enum;

        public abstract void Destroy();

        public abstract void Show();

        public abstract void Hide();

        public abstract void OnReady();

        public abstract void SetIndex(int index);
    }

    public class ViewCustomFactory : IViewCustomFactory
    {
        private readonly DiContainer _container;

        public ViewCustomFactory(DiContainer container)
        {
            _container = container;
        }

        public async UniTask<IViewContext> Create(IBaseModule module,
            string viewId,
            ViewName viewName)
        {
            BaseViewContext.Factory contextFactory = _container.ResolveId<BaseViewContext.Factory>(viewName);

            IViewContext instance = contextFactory.Create(viewId);
            await instance.TryCreateViewElement(module);
            return instance;
        }

        private void ValidateByEnumName<TEnumName, TClass>(DiContainer container) where TEnumName : Enum
        {
            Array values = Enum.GetValues(typeof(TEnumName));

            foreach (TEnumName e in values)
            {
                container.ResolveId<TClass>(e);
            }
        }

        public void Validate()
        {
            ValidateByEnumName<ViewName, BaseViewContext.Factory>(_container);
        }
    }
}