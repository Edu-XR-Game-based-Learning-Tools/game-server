using Core.Business;

namespace Core.View
{
    public class LoadingScript : IBaseScript
    {
        public BaseViewConfig GetConfig()
        {
            return new BaseViewConfig(
                bundle: $"{BaseViewScript.BasePath}/MRTK3/Utils/Loading.prefab",
                uiType: UIType.MRTK3
            )
            {
                Layer = LayerManager.Top,
                Config = ""
            };
        }
    }
}
