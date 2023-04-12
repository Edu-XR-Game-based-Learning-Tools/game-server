using Core.Business;
using UnityEngine;
using Zenject;

namespace Core.Framework
{
    public abstract class BasePoolObject : IPoolObject
    {
        protected readonly IBundleLoader _bundle;
        protected readonly IPoolManager _poolManager;

        public BasePoolObject(
            [Inject(Id = "PoolObjectContainer")]
            Transform poolObjectContainer,
            [Inject(Id = BundleLoaderName.Addressable)]
            IBundleLoader bundleLoader,
            [Inject(Id = PoolName.Object)]
            IPoolManager poolManager)
        {
            _bundle = bundleLoader;
            _poolManager = poolManager;
            PoolObjectContainer = poolObjectContainer;
        }

        public IGameObject ModelObj
        { get { return _modelObj; } set { _modelObj = (UGameObject)value; } }
        protected UGameObject _modelObj { get; set; }
        public Transform transform
        { get { return _modelObj.WrappedObj.transform; } }
        public Transform PoolObjectContainer { get; set; }

        public abstract void Reinitialize();

        public abstract void SelfDespawn(IPoolManager poolManager);

        public virtual void Destroy()
        {
            _bundle.ReleaseAsset(_modelObj.WrappedObj.name);
            _bundle.ReleaseInstance(_modelObj.WrappedObj);
            if (_modelObj.WrappedObj != null)
                GameObject.Destroy(_modelObj.WrappedObj);
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
}