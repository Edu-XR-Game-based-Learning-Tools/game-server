namespace Core.Business
{
    public interface ILoading : IBaseModule
    { }

    public class LoadingModel : IModuleContextModel
    {
        public string ViewId { get; set; }

        public IBaseModule Module { get; set; }

        public LoadingModel()
        { }

        public LoadingModel(string viewId)
        {
            ViewId = viewId;
        }

        public IModuleContextModel Clone()
        {
            return new LoadingModel(ViewId);
        }

        public void Refresh()
        {
            Module.Refresh(this);
        }

        public void CustomRefresh(string comparer)
        { }
    }
}
