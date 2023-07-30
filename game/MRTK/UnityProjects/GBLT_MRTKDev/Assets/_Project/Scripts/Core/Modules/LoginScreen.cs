using Core.Business;

namespace Core.Module
{
    public partial class LoginScreen : BaseModule, ILoginScreen
    {
        public enum ViewFunc
        {
            Refresh
        }

        public LoginScreenModel Model => _model;
        private LoginScreenModel _model;

        public LoginScreen()
        {
        }

        protected override void OnViewReady()
        { }

        protected override void OnDisposed()
        { }

        public override void Refresh(IModuleContextModel model)
        {
            _model = (LoginScreenModel)model;
            if (_model == null) return;

            ViewContext.Call(ViewFunc.Refresh);
        }
    }
}
