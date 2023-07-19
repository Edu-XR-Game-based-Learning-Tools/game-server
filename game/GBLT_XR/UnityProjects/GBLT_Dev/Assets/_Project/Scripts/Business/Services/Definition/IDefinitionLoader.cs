using Cysharp.Threading.Tasks;
using Shared.Network;

namespace Core.Business
{
    public interface IDefinitionLoader
    {
        UniTask<TDefinition[]> LoadDefinitions<TDefinition>() where TDefinition : class, IDefinition;
    }
}
