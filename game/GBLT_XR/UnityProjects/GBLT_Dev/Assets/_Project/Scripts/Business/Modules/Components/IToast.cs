namespace Core.Business
{
    public interface IToast : IBaseModule
    { }

    public class ToastModel : IModuleContextModel
    {
        public string ViewId { get; set; }

        public IBaseModule Module { get; set; }

        public ToastModel()
        { }

        public ToastModel(string viewId)
        {
            ViewId = viewId;
        }

        public IModuleContextModel Clone()
        {
            return new ToastModel(ViewId);
        }

        public void Refresh()
        {
            Module.Refresh(this);
        }

        public void CustomRefresh(string comparer)
        { }
    }
}
