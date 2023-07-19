using Core.Business;

namespace Core.Module
{
    public partial class RoomStatus : BaseModule, IRoomStatus
    {
        public enum ViewFunc
        {
            Refresh
        }

        public RoomStatusModel Model => _model;
        private RoomStatusModel _model;

        public RoomStatus()
        {
        }

        protected override void OnViewReady()
        { }

        protected override void OnDisposed()
        { }

        public override void Refresh(IModuleContextModel model)
        {
            _model = (RoomStatusModel)model;
            if (_model == null) return;

            ViewContext.Call(ViewFunc.Refresh);
        }
    }
}
