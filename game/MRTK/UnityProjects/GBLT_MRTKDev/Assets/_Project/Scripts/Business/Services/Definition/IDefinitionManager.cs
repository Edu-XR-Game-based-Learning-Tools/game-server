using Cysharp.Threading.Tasks;
using Shared.Network;
using System.Collections.Generic;

namespace Core.Business
{
    public interface IDefinitionManager
    {
        UniTask<TDefinition> GetDefinition<TDefinition>(string id) where TDefinition : class, IDefinition;

        UniTask<IList<TDefinition>> GetDefinitions<TDefinition>(string[] ids) where TDefinition : class, IDefinition;

        UniTask<IList<TDefinition>> GetAllDefinition<TDefinition>() where TDefinition : class, IDefinition;

        UniTask<IList<TDefinition>> LoadDefinitions<TDefinition>() where TDefinition : class, IDefinition;
    }
}
