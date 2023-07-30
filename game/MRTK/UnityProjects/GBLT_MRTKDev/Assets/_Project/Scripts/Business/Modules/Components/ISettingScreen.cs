namespace Core.Business
{
    public interface ISettingScreen : IBaseModule
    { }

    public class SettingScreenModel : IModuleContextModel
    {
        public string ViewId { get; set; }

        public IBaseModule Module { get; set; }

        public SettingScreenModel()
        { }

        public SettingScreenModel(string viewId)
        {
            ViewId = viewId;
        }

        public IModuleContextModel Clone()
        {
            return new SettingScreenModel(ViewId);
        }

        public void Refresh()
        {
            Module.Refresh(this);
        }

        public void CustomRefresh(string comparer)
        { }
    }
}
