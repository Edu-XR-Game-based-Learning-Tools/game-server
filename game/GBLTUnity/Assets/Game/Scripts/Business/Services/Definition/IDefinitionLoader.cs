using Cysharp.Threading.Tasks;

namespace Core.Business
{
    public interface IDefinitionLoader
    {
        UniTask<TDefinition[]> LoadDefinitions<TDefinition>() where TDefinition : class, IGameDefinition;
    }
}