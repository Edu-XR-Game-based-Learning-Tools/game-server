using Core.Business;
using VContainer.Unity;

namespace Core.Framework
{
    public abstract class TickablePoolObject : BasePoolObject, ITickable
    {
        protected TickablePoolObject(
            PoolObjectMono poolObjectContainer,
            IBundleLoader bundleLoader,
            IPoolManager poolManager) : base(poolObjectContainer, bundleLoader, poolManager)
        {
        }

        public abstract void Tick();
    }
}
