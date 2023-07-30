namespace Core.Business
{
    public interface IViewProxy
    {
        void Destroy();
        object Target { get; set; }
    }

    public interface IViewProxy<T> : IViewProxy
    {
        void Init();

        void ResetTarget(T newTarget);

        void RegisterDependencies();

        new T Target { get; set; }
    }
}
