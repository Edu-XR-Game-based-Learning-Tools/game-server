namespace Core.Business
{
    public interface IRoomStatus : IBaseModule
    { }

    public class RoomStatusModel : IModuleContextModel
    {
        public string ViewId { get; set; }

        public IBaseModule Module { get; set; }

        public RoomStatusModel()
        { }

        public RoomStatusModel(string viewId)
        {
            ViewId = viewId;
        }

        public IModuleContextModel Clone()
        {
            return new RoomStatusModel(ViewId);
        }

        public void Refresh()
        {
            Module.Refresh(this);
        }

        public void CustomRefresh(string comparer)
        { }
    }
}
