using Core.Business;

namespace Core.Module
{
    public partial class Toast : BaseModule, IToast
    {
        public enum ViewFunc
        {
            Refresh
        }

        public ToastModel Model => _model;
        private ToastModel _model;

        public Toast()
        {
        }

        protected override void OnViewReady()
        { }

        protected override void OnDisposed()
        { }

        public override void Refresh(IModuleContextModel model)
        {
            _model = (ToastModel)model;
            if (_model == null) return;

            ViewContext.Call(ViewFunc.Refresh);
        }
    }
}
