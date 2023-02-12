namespace Shared
{
    public class Defines
    {
        public const string USER_NAME_REGEX = "^[A-Z][A-Z0-9]{3,14}$";
        public const string USER_NAME_REGEX_ERROR =
                "The User Name must be followed these rules:" +
                "\nMust be unique. 4 to 15 characters." +
                "\nCannot start with number." +
                "\nNo space allowed.";
    }
}
