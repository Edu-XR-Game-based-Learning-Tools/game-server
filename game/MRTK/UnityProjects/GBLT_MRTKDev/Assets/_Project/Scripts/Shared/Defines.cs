namespace Shared
{
    public static class Defines
    {
        public const string USER_NAME_REGEX = "^[A-Z][A-Z0-9]{3,14}$";
        public const string USER_NAME_REGEX_ERROR =
                "The User Name must be followed these rules:" +
                "\nMust be unique. 4 to 15 characters." +
                "\nCannot start with number." +
                "\nNo space allowed.";
        public static string INVALID_SESSION = "Your session is invalid. Please sign in again.";
        public static string INVALID_PASSWORD = "Invalid Password!";

        // Room
        public static string INVALID_AMOUNT = "The capacity should be between 24 and 48.";
        public static string FULL_AMOUNT = "The room is FULL!";

        public static class PrefabKey
        {
            public const string DefaultRoomAvatar = "Assets/_Project/Bundles/Prefabs/Avatar/Person.prefab";
        }
    }
}
