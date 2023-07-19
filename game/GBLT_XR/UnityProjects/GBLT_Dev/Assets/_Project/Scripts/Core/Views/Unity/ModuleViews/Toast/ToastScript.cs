using Core.Business;

namespace Core.View
{
    public class ToastScript : IBaseScript
    {
        public BaseViewConfig GetConfig()
        {
            return new BaseViewConfig(
                bundle: $"{BaseViewScript.BasePath}/MRTK3/Utils/Toast.prefab",
                uiType: UIType.MRTK3
            )
            {
                Layer = LayerManager.Top,
                Config = ""
            };
        }
    }
}
