namespace Core.Business
{
    public enum ScreenName
    {
        SessionStart = 0,
        Home,
        Restart,
    }

    public enum ModuleName
    {
        Dummy,
        SplashScreen,
        LandingScreen,
        LoginScreen,
        ToolDescription,
        SettingScreen,
        RoomStatus,

        // Utils
        Loading,
        Popup,
        Toast
    }

    public enum ViewName
    {
        Unity
    }

    public enum BundleLoaderName
    {
        Resource,
        Addressable
    }

    public enum PoolName
    {
        Object,
        Audio
    }

    public enum DefinitionLocation
    {
        Local,
        Remote
    }
}
