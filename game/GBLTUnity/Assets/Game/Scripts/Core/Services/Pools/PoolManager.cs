using Core.Business;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using ILogger = Core.Business.ILogger;

namespace Core.Framework
{
    public class PoolManager : IPoolManager
    {
        private readonly Transform _poolObjectContainer;
        private readonly DiContainer _container;
        private readonly IBundleLoader _bundleLoader;
        private readonly TickableManager _tickableManager;
        private readonly ILogger _logger;

        private HashSet<string> _poolDic = new HashSet<string>();

        public PoolManager(
            [Inject(Id = "PoolObjectContainer")]
            Transform poolObjectContainer,
            TickableManager tickableManager,
            [Inject(Id = BundleLoaderName.Addressable)]
            IBundleLoader bundleLoader,
            DiContainer container,
            ILogger logger)
        {
            _poolObjectContainer = poolObjectContainer;
            _tickableManager = tickableManager;
            _bundleLoader = bundleLoader;
            _container = container;
            _logger = logger;
        }

        public async UniTask<T> GetObject<T>(string prefPath) where T : IPoolObject
        {
            MemoryPoolObject pool = GetPoolObject<T>(prefPath);
            T result = (T)pool.Spawn();
            if (result.ModelObj == null)
                await CreateUGoForObj(result, prefPath);

            result.Reinitialize();
            result.SetupPoolObjectContainer();
            SubcribeITickable(result);
            return result;
        }

        private async UniTask CreateUGoForObj<T>(T result, string prefPath) where T : IPoolObject
        {
            UGameObject UGo = await InstantiateUGameObject(prefPath);
            if (UGo == null)
                _logger.Error($"Cannot instantiate UGame Object for prefPath {prefPath}");
            AssignUGoToObj(UGo, result, prefPath);
        }

        private async UniTask<UGameObject> InstantiateUGameObject(string prefPath)
        {
            GameObject go = await _bundleLoader.InstantiateAssetAsync(prefPath);
            return new UGameObject(go);
        }

        private T AssignUGoToObj<T>(UGameObject UGo, T result, string prefPath) where T : IPoolObject
        {
            result.ModelObj = UGo;
            result.ModelObj.Name = prefPath;
            return result;
        }

        private void SubcribeITickable<T>(T obj)
        {
            ITickable iTic = obj as ITickable;
            if (iTic != null)
                _tickableManager.Add(iTic);
        }

        public void Despawn(IPoolObject obj)
        {
            _ = AsyncDespawn(obj);
        }

        private async UniTaskVoid AsyncDespawn(IPoolObject obj)
        {
            MemoryPoolObject pool = _container.ResolveId<MemoryPoolObject>(obj.ModelObj.Name);
            UnSubcribeITickable(obj);
            await UniTask.WaitForEndOfFrame(_poolObjectContainer.GetComponent<MonoBehaviour>());
            pool.Despawn(obj);
        }

        private void UnSubcribeITickable<T>(T obj)
        {
            ITickable iTic = obj as ITickable;
            if (iTic != null)
                _tickableManager.Remove(iTic);
        }

        private MemoryPoolObject GetPoolObject<T>(string prefPath) where T : IPoolObject
        {
            if (_poolDic.Contains(prefPath))
                return _container.ResolveId<MemoryPoolObject>(prefPath);
            else
                return CreateNewPool<T>(prefPath);
        }

        private MemoryPoolObject CreateNewPool<T>(string prefPath) where T : IPoolObject
        {
            _poolDic.Add(prefPath);
            _container.BindMemoryPool<IPoolObject, MemoryPoolObject>().WithId(prefPath).WithInitialSize(10).ExpandByDoubling().To<T>().AsCached();

            return _container.ResolveId<MemoryPoolObject>(prefPath);
        }

        public void ClearPool(IPoolObject obj)
        {
            ClearPool(obj.ModelObj.Name);
        }

        public void ClearPool(string prefabPath)
        {
            MemoryPoolObject pool = _container.ResolveId<MemoryPoolObject>(prefabPath);
            pool.Clear();
        }
    }
}