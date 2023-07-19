namespace Core.Business
{
    public interface ILandingScreen : IBaseModule
    { }

    public class LandingScreenModel : IModuleContextModel
    {
        public string ViewId { get; set; }

        public IBaseModule Module { get; set; }

        public LandingScreenModel()
        { }

        public LandingScreenModel(string viewId)
        {
            ViewId = viewId;
        }

        public IModuleContextModel Clone()
        {
            return new LandingScreenModel(ViewId);
        }

        public void Refresh()
        {
            Module.Refresh(this);
        }

        public void CustomRefresh(string comparer)
        { }
    }
}
