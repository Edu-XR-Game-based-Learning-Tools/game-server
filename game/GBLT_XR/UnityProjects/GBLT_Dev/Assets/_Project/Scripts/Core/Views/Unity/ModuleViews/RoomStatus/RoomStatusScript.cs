using Core.Business;

namespace Core.View
{
    public class RoomStatusScript : IBaseScript
    {
        public BaseViewConfig GetConfig()
        {
            return new BaseViewConfig(
                bundle: $"{BaseViewScript.BasePath}/MRTK3/RoomStatus/RoomStatus.prefab",
                uiType: UIType.MRTK3
            )
            {
                Layer = LayerManager.Main,
                Config = ""
            };
        }
    }
}
