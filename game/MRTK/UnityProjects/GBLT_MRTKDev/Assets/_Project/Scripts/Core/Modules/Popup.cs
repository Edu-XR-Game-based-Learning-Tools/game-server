using Core.Business;

namespace Core.Module
{
    public partial class Popup : BaseModule, IPopup
    {
        public enum ViewFunc
        {
            Refresh
        }

        public PopupModel Model => _model;
        private PopupModel _model;

        public Popup()
        {
        }

        protected override void OnViewReady()
        { }

        protected override void OnDisposed()
        { }

        public override void Refresh(IModuleContextModel model)
        {
            _model = (PopupModel)model;
            if (_model == null) return;

            ViewContext.Call(ViewFunc.Refresh);
        }
    }
}
