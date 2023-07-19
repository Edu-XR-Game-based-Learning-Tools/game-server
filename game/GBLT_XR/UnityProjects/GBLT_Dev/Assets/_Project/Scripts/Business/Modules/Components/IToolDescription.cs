namespace Core.Business
{
    public interface IToolDescription : IBaseModule
    { }

    public class ToolDescriptionModel : IModuleContextModel
    {
        public string ViewId { get; set; }

        public IBaseModule Module { get; set; }

        public ToolDescriptionModel()
        { }

        public ToolDescriptionModel(string viewId)
        {
            ViewId = viewId;
        }

        public IModuleContextModel Clone()
        {
            return new ToolDescriptionModel(ViewId);
        }

        public void Refresh()
        {
            Module.Refresh(this);
        }

        public void CustomRefresh(string comparer)
        { }
    }
}
