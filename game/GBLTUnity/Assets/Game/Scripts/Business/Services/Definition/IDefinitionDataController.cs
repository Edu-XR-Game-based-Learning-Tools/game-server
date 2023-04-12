using Cysharp.Threading.Tasks;

namespace Core.Business
{
    public interface IDefinitionDataController
    {
        public static GeneralConfigDefinition GeneralConfigDef { get; private set; }

        public UniTask VerifyClient();
    }
}