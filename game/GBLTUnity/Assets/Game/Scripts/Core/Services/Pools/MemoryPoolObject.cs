using Core.Business;
using Zenject;

namespace Core.Framework
{
    public class MemoryPoolObject : MemoryPool<IPoolObject>
    {
        protected override void OnSpawned(IPoolObject item)
        {
            base.OnSpawned(item);
            if (item.ModelObj != null)
                item.ModelObj.SetActive(true);
        }

        protected override void Reinitialize(IPoolObject item)
        {
            base.Reinitialize(item);
        }

        protected override void OnDespawned(IPoolObject item)
        {
            base.OnDespawned(item);
            item.BackToPool();
            item.ModelObj.SetActive(false);
        }

        protected override void OnDestroyed(IPoolObject item)
        {
            base.OnDestroyed(item);
            item.Destroy();
        }
    }
}