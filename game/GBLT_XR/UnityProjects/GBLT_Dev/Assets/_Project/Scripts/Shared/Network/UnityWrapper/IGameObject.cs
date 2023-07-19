namespace Shared.Network
{
    public interface IGameObject
    {
        string Name { get; set; } // Prefab path or key for example
        Vec3D Position { get; set; }
        void SetActive(bool value);
        bool IsActive { get; }
    }
}
