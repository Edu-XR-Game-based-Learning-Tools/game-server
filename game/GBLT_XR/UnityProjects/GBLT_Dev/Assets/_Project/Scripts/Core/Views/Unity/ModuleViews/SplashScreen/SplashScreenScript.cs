using Core.Business;

namespace Core.View
{
    public class SplashScreenScript : IBaseScript
    {
        public BaseViewConfig GetConfig()
        {
            return new BaseViewConfig(
                bundle: $"{BaseViewScript.BasePath}/MRTK3/SplashScreen/SplashScreen.prefab",
                uiType: UIType.MRTK3
            )
            {
                Layer = LayerManager.Main,
                Config = ""
            };
        }
    }
}
