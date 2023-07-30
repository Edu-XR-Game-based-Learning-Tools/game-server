namespace Core.Business
{
    public interface ILoginScreen : IBaseModule
    { }

    public class LoginScreenModel : IModuleContextModel
    {
        public string ViewId { get; set; }

        public IBaseModule Module { get; set; }

        public LoginScreenModel()
        { }

        public LoginScreenModel(string viewId)
        {
            ViewId = viewId;
        }

        public IModuleContextModel Clone()
        {
            return new LoginScreenModel(ViewId);
        }

        public void Refresh()
        {
            Module.Refresh(this);
        }

        public void CustomRefresh(string comparer)
        { }
    }
}
