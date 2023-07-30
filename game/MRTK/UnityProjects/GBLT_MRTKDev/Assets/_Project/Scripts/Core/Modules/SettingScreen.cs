using Core.Business;

namespace Core.Module
{
    public partial class SettingScreen : BaseModule, ISettingScreen
    {
        public enum ViewFunc
        {
            Refresh
        }

        public SettingScreenModel Model => _model;
        private SettingScreenModel _model;

        public SettingScreen()
        {
        }

        protected override void OnViewReady()
        { }

        protected override void OnDisposed()
        { }

        public override void Refresh(IModuleContextModel model)
        {
            _model = (SettingScreenModel)model;
            if (_model == null) return;

            ViewContext.Call(ViewFunc.Refresh);
        }
    }
}
