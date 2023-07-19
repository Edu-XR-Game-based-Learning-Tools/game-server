namespace Core.Business
{
    public interface ISplashScreen : IBaseModule
    { }

    public class SplashScreenModel : IModuleContextModel
    {
        public string ViewId { get; set; }

        public IBaseModule Module { get; set; }

        public SplashScreenModel()
        { }

        public SplashScreenModel(string viewId)
        {
            ViewId = viewId;
        }

        public IModuleContextModel Clone()
        {
            return new SplashScreenModel(ViewId);
        }

        public void Refresh()
        {
            Module.Refresh(this);
        }

        public void CustomRefresh(string comparer)
        { }
    }
}
