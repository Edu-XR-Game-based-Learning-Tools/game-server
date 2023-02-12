namespace Core.Business
{
    public interface IViewProxy
    {
        void Destroy();
        object Target { get; set; }
    }

    public interface IViewProxy<T> : IViewProxy
    {
        void Init(ILogger logger);

        void ResetTarget(T newTarget);

        void RegisterDependencies();

        new T Target { get; set; }
    }
}