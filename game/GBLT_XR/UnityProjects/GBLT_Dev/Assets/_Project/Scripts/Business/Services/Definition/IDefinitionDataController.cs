using Cysharp.Threading.Tasks;
using Shared.Network;

namespace Core.Business
{
    public interface IDefinitionDataController
    {
        public static GeneralConfigDefinition GeneralConfigDef { get; private set; }

        public UniTask VerifyClient();
    }
}
