using Core.Business;
using Core.EventSignal;
using Core.Framework;
using Cysharp.Threading.Tasks;
using MessagePipe;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using VContainer;

namespace Core.View
{
    public abstract class UnityView : MonoBehaviour
    {
        protected IBaseModule _module;

        public void SetModule(IBaseModule module)
        {
            _module = module;
        }

        private BaseViewScript _unityViewScript;

        public BaseViewScript BaseViewScript
        {
            get
            {
                return _unityViewScript;
            }
        }

        protected IBundleLoader _bundleLoader;
        protected AtlasManager _atlasManager;

        protected Dictionary<string, SpriteAtlas> _spriteBundlePathAndAtlasDict;

        public string PrefabBundlePath { get; set; }

        [Inject]
        protected readonly IPublisher<ShowLoadingSignal> _showLoadingPublisher;

        [Inject]
        protected readonly IPublisher<ShowToastSignal> _showToastPublisher;

        [Inject]
        protected readonly IPublisher<ShowPopupSignal> _showPopupPublisher;

        [Inject]
        public virtual void Construct(
            IObjectResolver container,
            AtlasManager atlasManager)
        {
            _atlasManager = atlasManager;
            _bundleLoader = container.Resolve<IReadOnlyList<IBundleLoader>>().ElementAt((int)BundleLoaderName.Addressable);
        }

        public void InnerInitialize(BaseViewScript unityViewScript)
        {
            _unityViewScript = unityViewScript;
            name = $"UnityView_{name}";
        }

        public async void SetImage(GameObject unityGo, string spritePath)
        {
            Image img = unityGo.GetComponentInChildren<Image>();
            img.sprite = await _atlasManager.GetSpriteFromDataAtlas(spritePath);
        }

        public bool IsSpriteBundlePathExist(string path)
        {
            if (_spriteBundlePathAndAtlasDict == null)
                _spriteBundlePathAndAtlasDict = new Dictionary<string, SpriteAtlas>();

            if (!_spriteBundlePathAndAtlasDict.ContainsKey(path))
            {
                _spriteBundlePathAndAtlasDict.Add(path, null);
                return false;
            }

            return true;
        }

        public async UniTask<SpriteAtlas> GetSpriteAtlas(string path)
        {
            var atlas = await _bundleLoader.LoadAssetAsync<SpriteAtlas>(path);
            _spriteBundlePathAndAtlasDict[path] = atlas;
            return atlas;
        }

        /// <summary>
        /// There is a case that multiple objects belong/beneath this UnityView would access to the
        /// Atlas at the same time. In that case, while the first object is making the loading call
        /// for the atlas, then the others have to wait until the atlas is loaded completely.
        /// </summary>
        public async UniTask<SpriteAtlas> WaitAtlasInSameUnityViewLoad(string path)
        {
            while (_spriteBundlePathAndAtlasDict[path] == null)
            {
                await UniTask.NextFrame();
            }

            return _spriteBundlePathAndAtlasDict[path];
        }

        private void OnDestroy()
        {
            StopAllCoroutines();

            if (PrefabBundlePath != null)
                _bundleLoader.ReleaseAsset(PrefabBundlePath);

            if (_spriteBundlePathAndAtlasDict != null)
            {
                var keys = _spriteBundlePathAndAtlasDict.Keys;
                foreach (var key in keys)
                    _bundleLoader.ReleaseAsset(key);

                _spriteBundlePathAndAtlasDict.Clear();
            }
        }

        public abstract void OnReady();
    }
}
