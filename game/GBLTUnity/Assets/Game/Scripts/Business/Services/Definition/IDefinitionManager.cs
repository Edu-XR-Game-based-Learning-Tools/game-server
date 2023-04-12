using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Core.Business
{
    public interface IDefinitionManager
    {
        UniTask<TDefinition> GetDefinition<TDefinition>(string id) where TDefinition : class, IGameDefinition;

        UniTask<IList<TDefinition>> GetDefinitions<TDefinition>(string[] ids) where TDefinition : class, IGameDefinition;

        UniTask<IList<TDefinition>> GetAllDefinition<TDefinition>() where TDefinition : class, IGameDefinition;

        UniTask<IList<TDefinition>> LoadDefinitions<TDefinition>() where TDefinition : class, IGameDefinition;
    }
}