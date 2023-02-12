namespace Core.Business
{
    public interface IScreenController
    {
        ScreenName Name { get; }

        bool IsAllowChangeScreen(ScreenName newScreen);

        void Enter();

        void Out();
    }
}