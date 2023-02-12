namespace Core.Business
{
    public interface IDummyUTKit : IBaseModule
    { }

    public class DummyUTKitModel : IModuleContextModel
    {
        public string ViewId { get; set; }

        public IBaseModule Module { get; set; }

        public DummyUTKitModel()
        { }

        public DummyUTKitModel(string viewId)
        {
            ViewId = viewId;
        }

        public IModuleContextModel Clone()
        {
            return new DummyUTKitModel(ViewId);
        }

        public void Refresh()
        {
            Module.Refresh(this);
        }

        public void CustomRefresh(string comparer)
        { }
    }
}