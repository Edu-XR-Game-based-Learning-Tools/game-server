namespace Core.Business
{
    public interface IPopup : IBaseModule
    { }

    public class PopupModel : IModuleContextModel
    {
        public string ViewId { get; set; }

        public IBaseModule Module { get; set; }

        public PopupModel()
        { }

        public PopupModel(string viewId)
        {
            ViewId = viewId;
        }

        public IModuleContextModel Clone()
        {
            return new PopupModel(ViewId);
        }

        public void Refresh()
        {
            Module.Refresh(this);
        }

        public void CustomRefresh(string comparer)
        { }
    }
}
