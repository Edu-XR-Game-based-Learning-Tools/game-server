using Core.Business;
using Core.Framework;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Zenject;
using ILogger = Core.Business.ILogger;

namespace Core.View
{
    public abstract class UnityView : MonoBehaviour
    {
        public class Factory : PlaceholderFactory<UnityViewScript, GameObject, string, UnityView>
        { }

        public class CustomFactory : IFactory<UnityViewScript, GameObject, string, UnityView>
        {
            private readonly DiContainer _container;

            public CustomFactory(DiContainer container)
            {
                _container = container;
            }

            public UnityView Create(UnityViewScript unityViewScript, GameObject prefab, string prefabBundlePath)
            {
                if (!prefab.TryGetComponent(out UnityView _))
                    return null;

                UnityView unityGo = _container.InstantiatePrefabForComponent<UnityView>(prefab);
                unityGo.InnerInitialize(unityViewScript);
                unityGo.PrefabBundlePath = prefabBundlePath;
                return unityGo;
            }
        }

        protected IBaseModule _module;
        public void SetModule(IBaseModule module)
        {
            _module = module;
        }

        private UnityViewScript _unityViewScript;
        public UnityViewScript UnityViewScript
        {
            get
            {
                return _unityViewScript;
            }
        }

        protected ILogger _logger;
        protected IBundleLoader _bundle;
        protected AtlasManager _atlasManager;

        protected Dictionary<string, SpriteAtlas> _spriteBundlePathAndAtlasDict;

        public string PrefabBundlePath { get; set; }

        [Inject]
        public virtual void Construct(
            ILogger logger,
            [Inject(Id = BundleLoaderName.Addressable)]
            IBundleLoader bundle,
            AtlasManager atlasManager)
        {
            _logger = logger;
            _bundle = bundle;
            _atlasManager = atlasManager;
        }

        private void InnerInitialize(UnityViewScript unityViewScript)
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
            var atlas = await _bundle.LoadAssetAsync<SpriteAtlas>(path);
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
                _bundle.ReleaseAsset(PrefabBundlePath);

            if (_spriteBundlePathAndAtlasDict != null)
            {
                var keys = _spriteBundlePathAndAtlasDict.Keys;
                foreach (var key in keys)
                    _bundle.ReleaseAsset(key);

                _spriteBundlePathAndAtlasDict.Clear();
            }
        }

        public abstract void OnReady();
    }
}