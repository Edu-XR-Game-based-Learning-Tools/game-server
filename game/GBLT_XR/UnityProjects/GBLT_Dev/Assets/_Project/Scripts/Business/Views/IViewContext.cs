using Core.View;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace Core.Business
{
    public enum DefaultFunc
    {
        OnReady,
        GetConfig,
        Show,
        Hide,
        SetIndex,
    }

    public class BaseViewConfig
    {
        public string Bundle { get; set; }
        public UIType UIType { get; set; }
        public BaseViewConfig(string bundle, UIType uiType)
        {
            Bundle = bundle;
            UIType = uiType;
        }

        public string ViewId;
        public string Config;
        public LayerManager Layer = LayerManager.None;
        public AnchorPresets AnchorPreset = AnchorPresets.StretchAll;
        public Vector2 AnchorPos = Vector2.zero;
        public Vector2 SizeDelta = Vector2.zero;
        public bool KeepLayout;
    }

    public interface IViewContext
    {
        BaseViewConfig ConfigModel { get; set; }
        IBaseModule Module { get; set; }
        GameObject View { get; }

        BaseViewContext Create(
            string viewId,
            IObjectResolver container);

        UniTask TryCreateViewElement(IBaseModule module);

        void Call<T>(T function, params object[] args) where T : Enum;

        void Destroy();

        void Show();

        void Hide();

        void OnReady();

        void SetIndex(int index);
    }

    public abstract class BaseViewContext : IViewContext
    {
        public class Factory
        {
            private readonly IObjectResolver _container;

            public Factory(IObjectResolver container)
            {
                _container = container;
            }

            public IViewContext Create(string viewId, ViewName viewName)
            {
                IReadOnlyList<IViewContext> viewContexts = _container.Resolve<IReadOnlyList<IViewContext>>();
                BaseViewContext viewContext = (BaseViewContext)viewContexts.ElementAt((int)viewName);
                return viewContext.Create(viewId, _container);
            }
        }

        protected string _viewId;
        protected ViewScriptManager _viewScriptManager;
        protected IBundleLoader _bundleLoader;
        protected IObjectResolver _container;

        public BaseViewConfig ConfigModel { get; set; }

        public abstract GameObject View { get; }
        public IBaseModule Module { get; set; }

        public BaseViewContext Create(
            string viewId,
            IObjectResolver container)
        {
            _viewId = viewId;
            _container = container;
            _viewScriptManager = container.Resolve<ViewScriptManager>();
            _bundleLoader = container.Resolve<IReadOnlyList<IBundleLoader>>().ElementAt((int)BundleLoaderName.Addressable);

            return this;
        }

        public abstract UniTask TryCreateViewElement(IBaseModule module);

        public abstract void Call<T>(T function, params object[] args) where T : Enum;

        public abstract void Destroy();

        public abstract void Show();

        public abstract void Hide();

        public abstract void OnReady();

        public abstract void SetIndex(int index);
    }

    public interface IBaseScript
    {
        BaseViewConfig GetConfig();
    }
}
