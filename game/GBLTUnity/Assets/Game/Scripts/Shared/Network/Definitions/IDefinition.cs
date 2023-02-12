namespace Shared.Network
{
    public interface IDefinition
    {
        string Id { get; set; }
    }

    [System.Serializable]
    public abstract class BaseDefinition : IDefinition
    {
        public abstract string Id { get; set; }
    }

    [System.Serializable]
    public abstract class BaseItemDefinition : BaseDefinition
    {
        public string Name;
    }
}