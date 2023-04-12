using Cysharp.Threading.Tasks;

namespace Core.Business
{
    public interface IPoolManager
    {
        UniTask<T> GetObject<T>(string prefPath) where T : IPoolObject;
        void Despawn(IPoolObject obj);
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
