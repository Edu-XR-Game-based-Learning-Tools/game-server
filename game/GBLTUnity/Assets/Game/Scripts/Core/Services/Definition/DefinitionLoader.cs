using Core.Business;
using Cysharp.Threading.Tasks;

namespace Core.Framework
{
    public class DefinitionLoader : IDefinitionLoader
    {
        private readonly ILogger _logger;
        private readonly IFileReader _fireReader;

        public DefinitionLoader(
            ILogger logger,
            IFileReader fireReader)
        {
            _fireReader = fireReader;
            _logger = logger;
        }

        public UniTask<TDefinition[]> LoadDefinitions<TDefinition>() where TDefinition : class, IGameDefinition
        {
            return _fireReader.Read<TDefinition[]>(ConfigFileName.GetFileName<TDefinition>());
        }
    }
}