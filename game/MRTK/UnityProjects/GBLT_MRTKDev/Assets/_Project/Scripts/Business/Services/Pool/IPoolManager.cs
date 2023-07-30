using Cysharp.Threading.Tasks;
using Shared.Network;

namespace Core.Business
{
    public interface IPoolManager
    {
        UniTask<T> GetObject<T>(string prefPath) where T : IPoolObject;

        void DeSpawn(IPoolObject obj);

        void ClearPool(IPoolObject obj);

        void ClearPool(string prefabPath);
    }

    public interface IPoolObject
    {
        IGameObject ModelObj { get; set; }

        void Reinitialize();

        void Destroy();

        void BackToPool();

        void SetupPoolObjectContainer();
    }
}
