using Core.Business;
using Cysharp.Threading.Tasks;
using Shared.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.Framework
{
    public class BasePoolObject : IPoolObject
    {
        public class Factory
        {
            private readonly PoolObjectMono _poolObjectContainer;
            private readonly IObjectResolver _container;

            public Factory(
                PoolObjectMono poolObjectContainer,
                IObjectResolver container)
            {
                _poolObjectContainer = poolObjectContainer;
                _container = container;
            }

            public IPoolObject Create(PoolName poolName)
            {
                var poolManager = _container.Resolve<IReadOnlyList<IPoolManager>>().ElementAt((int)poolName);
                var bundleLoader = _container.Resolve<IReadOnlyList<IBundleLoader>>().ElementAt((int)BundleLoaderName.Addressable);
                var obj = new BasePoolObject(_poolObjectContainer, bundleLoader, poolManager);
                return obj;
            }
        }

        protected readonly IBundleLoader _bundle;
        protected readonly IPoolManager _poolManager;

        public BasePoolObject(
            PoolObjectMono poolObjectContainer,
            IBundleLoader bundleLoader,
            IPoolManager poolManager)
        {
            PoolObjectContainer = poolObjectContainer.transform;
            _bundle = bundleLoader;
            _poolManager = poolManager;
        }

        public IGameObject ModelObj
        { get { return _modelObj; } set { _modelObj = (UGameObject)value; } }

        protected UGameObject _modelObj { get; set; }

        public Transform transform
        { get { return _modelObj.WrappedObj.transform; } }

        public Transform PoolObjectContainer { get; set; }

        public virtual void Reinitialize()
        { }

        public virtual void SelfDespawn(IPoolManager poolManager)
        { }

        public virtual void Destroy()
        {
            _bundle.ReleaseAsset(_modelObj.WrappedObj.name);
            _bundle.ReleaseInstance(_modelObj.WrappedObj);
            if (_modelObj.WrappedObj != null)
                UnityEngine.Object.Destroy(_modelObj.WrappedObj);
        }

        public void BackToPool()
        {
            _modelObj.SetParent(PoolObjectContainer);
        }

        public void SetupPoolObjectContainer()
        {
            _modelObj.SetParent(PoolObjectContainer);
        }
    }

    public abstract class BasePoolManager : IPoolManager
    {
        protected readonly IObjectResolver _container;
        protected readonly IBundleLoader _bundleLoader;

        protected readonly Dictionary<string, ObjectPool<IPoolObject>> _prefabLookup = new(); // To store pool of specific key
        protected readonly Dictionary<IPoolObject, ObjectPool<IPoolObject>> _instanceLookup = new(); // To track from instance to which pool

        protected virtual PoolName PoolName => PoolName.Object;

        //protected bool _logStatus;
        protected bool _dirtyLog = false;

        public BasePoolManager(IObjectResolver container)
        {
            _container = container;
            _bundleLoader = _container.Resolve<IReadOnlyList<IBundleLoader>>().ElementAt((int)BundleLoaderName.Addressable);
        }

        protected async UniTask<UGameObject> InstantiateUGameObject(string prefPath)
        {
            GameObject prefab = await _bundleLoader.LoadAssetAsync<GameObject>(prefPath);
            var go = _container.Instantiate(prefab);
            UGameObject uGo = new(go)
            {
                Name = prefPath
            };
            return uGo;
        }

        protected async UniTask<T> InstantiatePoolObject<T>(string prefPath) where T : IPoolObject
        {
            BasePoolObject.Factory factory = _container.Resolve<BasePoolObject.Factory>();
            T obj = (T)factory.Create(PoolName);
            obj.ModelObj = await InstantiateUGameObject(prefPath);
            return obj;
        }

        protected async UniTask WarmPool<T>(string prefPath, int size = 1) where T : IPoolObject
        {
            if (_prefabLookup.ContainsKey(prefPath))
            {
                throw new Exception("Pool for prefab " + prefPath + " has already been created");
            }
            var pool = new ObjectPool<IPoolObject>(async () => await InstantiatePoolObject<T>(prefPath), size);
            _prefabLookup[prefPath] = pool;
            await pool.Warm(size);

            _dirtyLog = true;
        }

        public virtual UniTask<T> GetObject<T>(string prefPath) where T : IPoolObject
        {
            throw new NotImplementedException();
        }

        public virtual void DeSpawn(IPoolObject obj)
        {
            obj.ModelObj.SetActive(false);

            if (_instanceLookup.ContainsKey(obj))
            {
                _instanceLookup[obj].ReleaseItem(obj);
                _instanceLookup.Remove(obj);
            }
        }

        public virtual void ClearPool(IPoolObject obj)
        {
            ClearPool(obj.ModelObj.Name);
        }

        public virtual void ClearPool(string prefabPath)
        {
            if (!_prefabLookup.ContainsKey(prefabPath)) return;
            foreach (var obj in _prefabLookup[prefabPath].List)
                DeSpawn(obj.Item);
            _prefabLookup[prefabPath].Clear();
        }

        //public void PrintStatus()
        //{
        //    foreach (KeyValuePair<string, ObjectPool<IPoolObject>> keyVal in _prefabLookup)
        //    {
        //        Debug.Log(string.Format("Object Pool for Prefab: {0} In Use: {1} Total {2}", keyVal.Key, keyVal.Value.CountUsedItems, keyVal.Value.Count));
        //    }
        //}

        //private void Update()
        //{
        //    if (_logStatus && _dirtLogy)
        //    {
        //        PrintStatus();
        //        _dirtyLog = false;
        //    }
        //}
    }
}
