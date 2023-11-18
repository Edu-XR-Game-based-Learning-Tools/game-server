namespace Core.Business
{
    public interface IQuizzesRoomStatus : IBaseModule
    { }

    public class QuizzesRoomStatusModel : IModuleContextModel
    {
        public string ViewId { get; set; }

        public IBaseModule Module { get; set; }

        public QuizzesRoomStatusModel()
        { }

        public QuizzesRoomStatusModel(string viewId)
        {
            ViewId = viewId;
        }

        public IModuleContextModel Clone()
        {
            return new QuizzesRoomStatusModel(ViewId);
        }

        public void Refresh()
        {
            Module.Refresh(this);
        }

        public void CustomRefresh(string comparer)
        { }
    }
}
