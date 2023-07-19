using Core.Business;

namespace Core.Module
{
    public partial class ToolDescription : BaseModule, IToolDescription
    {
        public enum ViewFunc
        {
            Refresh
        }

        public ToolDescriptionModel Model => _model;
        private ToolDescriptionModel _model;

        public ToolDescription()
        {
        }

        protected override void OnViewReady()
        { }

        protected override void OnDisposed()
        { }

        public override void Refresh(IModuleContextModel model)
        {
            _model = (ToolDescriptionModel)model;
            if (_model == null) return;

            ViewContext.Call(ViewFunc.Refresh);
        }
    }
}
