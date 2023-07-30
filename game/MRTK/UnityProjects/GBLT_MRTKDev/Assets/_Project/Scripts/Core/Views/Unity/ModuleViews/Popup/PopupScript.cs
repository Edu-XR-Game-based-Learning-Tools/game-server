using Core.Business;

namespace Core.View
{
    public class PopupScript : IBaseScript
    {
        public BaseViewConfig GetConfig()
        {
            return new BaseViewConfig(
                bundle: $"{BaseViewScript.BasePath}/MRTK3/Utils/Popup.prefab",
                uiType: UIType.MRTK3
            )
            {
                Layer = LayerManager.Top,
                Config = ""
            };
        }
    }
}
