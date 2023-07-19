using Core.Business;

namespace Core.View
{
    public class LandingScreenScript : IBaseScript
    {
        public BaseViewConfig GetConfig()
        {
            return new BaseViewConfig(
                bundle: $"{BaseViewScript.BasePath}/MRTK3/LandingScreen/LandingScreen.prefab",
                uiType: UIType.MRTK3
            )
            {
                Layer = LayerManager.Main,
                Config = ""
            };
        }
    }
}
