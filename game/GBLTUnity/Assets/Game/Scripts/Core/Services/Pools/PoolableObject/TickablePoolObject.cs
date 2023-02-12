using Core.Business;
using UnityEngine;
using Zenject;

namespace Core.Framework
{
    public abstract class TickablePoolObject : BasePoolObject, ITickable
    {
        protected TickablePoolObject(
            [Inject(Id = "PoolObjectContainer")]
            Transform poolObjectContainer,
            [Inject(Id = BundleLoaderName.Addressable)]
            IBundleLoader bundleLoader,
            [Inject(Id = PoolName.Object)]
            IPoolManager poolManager) : base(poolObjectContainer, bundleLoader, poolManager)
        {
        }

        public abstract void Tick();
    }
}