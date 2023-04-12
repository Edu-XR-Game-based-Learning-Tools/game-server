using System;

namespace Core.Business
{
    public interface IGameDefinition
    {
        string Id { get; set; }
    }

    [Serializable]
    public abstract class BaseItemDefinition : IGameDefinition
    {
        public abstract string Id { get; set; }
        public string Name;
    }
}