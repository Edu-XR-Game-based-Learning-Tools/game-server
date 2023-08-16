using Core.Business;
using UnityEngine;

namespace Core.Module
{
    public partial class SplashScreen : BaseModule, ISplashScreen
    {
        public enum ViewFunc
        {
            Refresh
        }

        public SplashScreenModel Model => _model;
        private SplashScreenModel _model;

        public SplashScreen()
        {
        }

        protected override void OnViewReady()
        {
        }

        protected override void OnDisposed()
        { }

        public override void Refresh(IModuleContextModel model)
        {
            _model = (SplashScreenModel)model;
            if (_model == null) return;

            ViewContext.Call(ViewFunc.Refresh);
        }
    }
}
