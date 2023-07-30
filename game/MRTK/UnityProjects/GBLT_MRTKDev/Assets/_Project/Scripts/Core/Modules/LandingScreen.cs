using Core.Business;

namespace Core.Module
{
    public partial class LandingScreen : BaseModule, ILandingScreen
    {
        public enum ViewFunc
        {
            Refresh
        }

        public LandingScreenModel Model => _model;
        private LandingScreenModel _model;

        public LandingScreen()
        {
        }

        protected override void OnViewReady()
        { }

        protected override void OnDisposed()
        { }

        public override void Refresh(IModuleContextModel model)
        {
            _model = (LandingScreenModel)model;
            if (_model == null) return;

            ViewContext.Call(ViewFunc.Refresh);
        }
    }
}
