using Core.Business;

namespace Core.View
{
    public class DummyXRScript : IBaseScript
    {
        public BaseViewConfig GetConfig()
        {
            return new BaseViewConfig(
                bundle: "Assets/Game/Bundles/Prefabs/Views/MRTK3/Dummy/View.prefab",
                uiType: UIType.MRTK3
            )
            {
                Config = ""
            };
        }
    }
}