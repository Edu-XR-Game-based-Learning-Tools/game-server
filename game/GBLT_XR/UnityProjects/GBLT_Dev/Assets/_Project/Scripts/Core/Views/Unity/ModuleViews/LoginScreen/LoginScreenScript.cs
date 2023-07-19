using Core.Business;

namespace Core.View
{
    public class LoginScreenScript : IBaseScript
    {
        public BaseViewConfig GetConfig()
        {
            return new BaseViewConfig(
                bundle: $"{BaseViewScript.BasePath}/MRTK3/LoginScreen/LoginScreen.prefab",
                uiType: UIType.MRTK3
            )
            {
                Layer = LayerManager.Main,
                Config = ""
            };
        }
    }
}
