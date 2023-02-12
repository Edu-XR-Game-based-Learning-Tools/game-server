using Core.Business;
using Core.Extension;
using Cysharp.Threading.Tasks;
using Shared.Extension;
using System.Collections;
using UnityEngine;
using Zenject;
using ILogger = Core.Business.ILogger;

namespace Core.View
{
    public class UnityViewContext : BaseViewContext
    {
        private readonly ILogger _logger;
        private readonly string _viewId;
        private readonly UnityViewScriptManager<UnityViewScript> _unityScriptManager;
        private readonly UnityView.Factory _unityGOFactory;
        private readonly IBundleLoader _bundleLoader;
        private readonly DiContainer _diContainer;

        private UnityView _unityGo;
        private UnityViewScript _unityViewScript;

        public override GameObject View
        {
            get
            {
                if (ReferenceEquals(_unityGo, null) || _unityGo == null) return null;
                return _unityGo.gameObject;
            }
        }

        public UnityViewContext(
            string viewId,
            ILogger logger,
            DiContainer diContainer,
            UnityViewScriptManager<UnityViewScript> unityScriptManager,
            UnityView.Factory unityGOFactory,
            [Inject(Id = BundleLoaderName.Addressable)]
            IBundleLoader bundleLoader)
        {
            _viewId = viewId;
            _logger = logger;
            _diContainer = diContainer;
            _unityScriptManager = unityScriptManager;
            _unityGOFactory = unityGOFactory;
            _bundleLoader = bundleLoader;
        }

        private void LoadConfigModel()
        {
            ConfigModel = _unityViewScript.GetConfig();
        }

        public override async UniTask TryCreateViewElement(IBaseModule module)
        {
            _unityViewScript = _unityScriptManager.GetScript(_viewId);
            LoadConfigModel();
            await CreateUnityElement(module, ConfigModel);
        }

        private async UniTask CreateUnityElement(IBaseModule module, BaseViewConfig configModel)
        {
            GameObject viewPref = await LoadPrefab(configModel.Bundle);
            _unityGo = _unityGOFactory.Create(_unityViewScript, viewPref, configModel.Bundle);
            _unityGo.SetModule(module);
            await UniTask.DelayFrame(1);
            ApplyViewConfig(ConfigModel);
        }

        private void ApplyViewConfig(BaseViewConfig config)
        {
            IViewLayerManager viewLayerManager = _diContainer.ResolveId<IViewLayerManager>(config.UIType.ToString());
            View.transform.SetParent(viewLayerManager.GetLayerRoot(config.Layer));
            if (_unityGo.gameObject.activeInHierarchy)
                _unityGo.StartCoroutine(ExpandCanvas());
        }

        private IEnumerator ExpandCanvas()
        {
            View.transform.localPosition = Vector3.zero;
            View.transform.localScale = Vector3.one;
            if (!ConfigModel.KeepLayout)
            {
                RectTransform rect = View.GetComponent<RectTransform>();
                rect.SetAnchor(ConfigModel.AnchorPreset);
                rect.sizeDelta = ConfigModel.SizeDelta;
                rect.anchoredPosition = new Vector3(ConfigModel.AnchorPos.x, ConfigModel.AnchorPos.y, 0);
            }

            yield return CoShow();
        }

        private async UniTask<GameObject> LoadPrefab(string path)
        {
            GameObject viewPref = await _bundleLoader.LoadAssetAsync<GameObject>(path);
            return viewPref;
        }

        public override void Call<T>(T function, params object[] args)
        {
            _unityGo.CallMethod(function.ToString(), args);
        }

        public override void OnReady()
        {
            _unityGo.CallMethod(DefaultFunc.OnReady.ToString());
        }

        public override void SetIndex(int index)
        {
            if (!_unityGo.HasMethod(DefaultFunc.SetIndex.ToString())) return;
            _unityGo.CallMethod(DefaultFunc.SetIndex.ToString(), index);
        }

        public override void Show()
        {
            if (!_unityGo.HasMethod(DefaultFunc.Show.ToString())) return;
            _unityGo.CallMethod(DefaultFunc.Show.ToString());
        }

        private IEnumerator CoShow()
        {
            if (!_unityGo.HasMethod(DefaultFunc.Show.ToString())) yield break;
            yield return _unityGo.StartCoroutine(DefaultFunc.Show.ToString());
        }

        public override void Hide()
        {
            if (!_unityGo.HasMethod(DefaultFunc.Hide.ToString())) return;
            _unityGo.CallMethod(DefaultFunc.Hide.ToString());
        }

        private IEnumerator CoHide()
        {
            if (!_unityGo.HasMethod(DefaultFunc.Hide.ToString())) yield break;
            yield return _unityGo.StartCoroutine(DefaultFunc.Hide.ToString());
        }

        public override void Destroy()
        {
            if (_unityGo.gameObject.activeInHierarchy)
                _unityGo.StartCoroutine(CoDestroy());
        }

        private IEnumerator CoDestroy()
        {
            yield return CoHide();
            Object.Destroy(_unityGo.gameObject);
        }
    }
}