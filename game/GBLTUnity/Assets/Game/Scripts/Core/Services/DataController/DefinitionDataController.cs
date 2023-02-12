using Core.Business;

using Cysharp.Threading.Tasks;

namespace Core.Framework {

    public class DefinitionDataController : IDefinitionDataController {
        public static GeneralConfigDefinition GeneralConfigDef { get; private set; } = new GeneralConfigDefinition();

        private readonly ILogger _logger;
        private readonly IDefinitionManager _definitionManager;

        public DefinitionDataController(
            ILogger logger,
            IDefinitionManager definitionManager) {
            _logger = logger;
            _definitionManager = definitionManager;
        }

        public async UniTask VerifyClient() {
            await GetGeneralConfig();
        }

        public async UniTask GetGeneralConfig() {
            GeneralConfigDef = await _definitionManager.GetDefinition<GeneralConfigDefinition>("Default");
        }
    }
}