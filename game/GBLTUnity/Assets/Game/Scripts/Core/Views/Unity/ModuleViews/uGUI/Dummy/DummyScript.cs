using Core.Business;

namespace Core.View
{
    public class DummyScript : IBaseScript
    {
        public BaseViewConfig GetConfig()
        {
            return new BaseViewConfig(
                bundle: "Assets/Game/Bundles/Prefabs/Views/uGUI/Dummy/Dummy.prefab",
                uiType: UIType.uGUI
            )
            {
                Layer = LayerManager.Main
            };
        }
    }
}