using Core.Business;

namespace Core.Module
{
    public partial class Loading : BaseModule, ILoading
    {
        public enum ViewFunc
        {
            Refresh
        }

        public LoadingModel Model => _model;
        private LoadingModel _model;

        public Loading()
        {
        }

        protected override void OnViewReady()
        { }

        protected override void OnDisposed()
        { }

        public override void Refresh(IModuleContextModel model)
        {
            _model = (LoadingModel)model;
            if (_model == null) return;

            ViewContext.Call(ViewFunc.Refresh);
        }
    }
}
