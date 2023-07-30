using Core.Business;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Core.Framework
{
    public class PoolManager : BasePoolManager
    {
        public PoolManager(IObjectResolver container) : base(container)
        {
        }

        public override async UniTask<T> GetObject<T>(string prefPath)
        {
            return await GetObject<T>(prefPath, Vector3.zero, Quaternion.identity);
        }

        private async UniTask<T> GetObject<T>(string prefPath, Vector3 position, Quaternion rotation) where T : IPoolObject
        {
            if (!_prefabLookup.ContainsKey(prefPath))
            {
                await WarmPool<T>(prefPath, 1);
            }

            var pool = _prefabLookup[prefPath];

            var obj = await pool.GetItem();
            obj.Reinitialize();
            obj.SetupPoolObjectContainer();
            obj.ModelObj.SetActive(true);

            _instanceLookup.Add(obj, pool);
            _dirtyLog = true;
            return (T)obj;
        }
    }
}
