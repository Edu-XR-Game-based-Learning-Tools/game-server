using Core.Business;

namespace Core.Module
{
    public partial class Dummy : BaseModule, IDummy
    {
        public enum ViewFunc
        {
            Refresh
        }

        public DummyModel Model => _model;
        private DummyModel _model;

        public Dummy()
        {
        }

        protected override void OnViewReady()
        { }

        protected override void OnDisposed()
        { }

        public override void Refresh(IModuleContextModel model)
        {
            _model = (DummyModel)model;
            if (_model == null) return;

            ViewContext.Call(ViewFunc.Refresh);
        }
    }
}
