using Core.Business;

using Cysharp.Threading.Tasks;
using Shared.Network;

namespace Core.Framework
{
    public class DefinitionDataController : IDefinitionDataController
    {
        public static GeneralConfigDefinition GeneralConfigDef { get; private set; } = new GeneralConfigDefinition();

        private readonly IUserDataController _userDataController;
        private readonly IDefinitionManager _definitionManager;

        public DefinitionDataController(
            IUserDataController userDataController,
            IDefinitionManager definitionManager)
        {
            _userDataController = userDataController;
            _definitionManager = definitionManager;
        }

        public async UniTask VerifyClient()
        {
            await GetGeneralConfig();
            _userDataController.CacheDefinitions();
        }

        public async UniTask GetGeneralConfig()
        {
            GeneralConfigDef = await _definitionManager.GetDefinition<GeneralConfigDefinition>("Default");
        }
    }
}
