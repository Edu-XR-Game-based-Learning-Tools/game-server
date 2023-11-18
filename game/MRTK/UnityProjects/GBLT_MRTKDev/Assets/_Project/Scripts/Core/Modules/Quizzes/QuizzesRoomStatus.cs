using Core.Business;

namespace Core.Module
{
    public partial class QuizzesRoomStatus : BaseModule, IQuizzesRoomStatus
    {
        public enum ViewFunc
        {
            Refresh
        }

        public QuizzesRoomStatusModel Model => _model;
        private QuizzesRoomStatusModel _model;

        public QuizzesRoomStatus()
        {
        }

        protected override void OnViewReady()
        { }

        protected override void OnDisposed()
        { }

        public override void Refresh(IModuleContextModel model)
        {
            _model = (QuizzesRoomStatusModel)model;
            if (_model == null) return;

            ViewContext.Call(ViewFunc.Refresh);
        }
    }
}
