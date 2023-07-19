using Core.Business;
using Cysharp.Threading.Tasks;
using Shared.Network;

namespace Core.Framework
{
    public class DefinitionLoader : IDefinitionLoader
    {
        private readonly IFileReader _fireReader;

        public DefinitionLoader(
            IFileReader fireReader)
        {
            _fireReader = fireReader;
        }

        public UniTask<TDefinition[]> LoadDefinitions<TDefinition>() where TDefinition : class, IDefinition
        {
            return _fireReader.Read<TDefinition[]>(ConfigFileName.GetFileName<TDefinition>());
        }
    }
}
