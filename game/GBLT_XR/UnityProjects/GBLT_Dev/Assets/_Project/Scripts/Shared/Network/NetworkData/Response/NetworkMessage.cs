namespace Shared.Network
{
    using MessagePack;

    public static class Enums
    {
        public enum Role
        {
            Admin,
            Basic
        }
    }

    public static class Constants
    {
        public const string AdminRole = "Admin";
        public const string BasicRole = "Basic";

        public const string AuthToken = "authToken";

        public static class Url
        {
            public const string Login = "auth/login";
            public const string Register = "auth/register";
            public const string RefreshToken = "auth/refreshtoken";
        }
    }

    public class BaseUrlConfiguration
    {
        public const string CONFIG_NAME = "baseUrls";

        public string ApiBase { get; set; }
    }

    [MessagePackObject(true)]
    public class GeneralResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; } = true;
    }
}
