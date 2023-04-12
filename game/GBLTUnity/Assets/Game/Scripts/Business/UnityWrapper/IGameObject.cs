namespace Core.Business
{
    public interface IGameObject
    {
        string Name { get; set; }
        Vec3D Position { get; set; }
        void SetActive(bool value);
        bool IsActive { get; }
    }
}
