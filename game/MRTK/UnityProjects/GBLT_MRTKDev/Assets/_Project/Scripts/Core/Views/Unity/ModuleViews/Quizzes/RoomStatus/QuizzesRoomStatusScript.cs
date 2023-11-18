using Core.Business;

namespace Core.View
{
    public class QuizzesRoomStatusScript : IBaseScript
    {
        public BaseViewConfig GetConfig()
        {
            return new BaseViewConfig(
                bundle: $"{BaseViewScript.BasePath}/MRTK3/Quizzes/RoomStatus/RoomStatus.prefab",
                uiType: UIType.MRTK3
            )
            {
                Layer = LayerManager.Main,
                Config = ""
            };
        }
    }
}
