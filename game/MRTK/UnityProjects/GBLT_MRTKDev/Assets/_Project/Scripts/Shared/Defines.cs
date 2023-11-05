namespace Shared
{
    using System;
    using System.Linq;

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
            public static string[] AvatarPaths = new string[] {
                "Assets/_Project/Bundles/Graphics/Avatar/row-1-column-1.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-1-column-2.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-1-column-3.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-1-column-4.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-1-column-5.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-2-column-1.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-2-column-2.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-2-column-3.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-2-column-4.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-2-column-5.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-3-column-1.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-3-column-2.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-3-column-3.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-3-column-4.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-3-column-5.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-4-column-1.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-4-column-2.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-4-column-3.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-4-column-4.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-4-column-5.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-5-column-1.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-5-column-2.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-5-column-3.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-5-column-4.png",
                "Assets/_Project/Bundles/Graphics/Avatar/row-5-column-5.png",
            };

            public static string[] ModelThumbnailPaths = new string[] {
                "Assets/_Project/Bundles/Prefabs/Avatar/Person.png",
                "Assets/_Project/Bundles/Prefabs/Avatar/Person 1.png",
                "Assets/_Project/Bundles/Prefabs/Avatar/Person 2.png",
                "Assets/_Project/Bundles/Prefabs/Avatar/Person 3.png",
            };

            public static string[] ModelPaths = new string[] {
                "Assets/_Project/Bundles/Prefabs/Avatar/Person.prefab",
                "Assets/_Project/Bundles/Prefabs/Avatar/Person 1.prefab",
                "Assets/_Project/Bundles/Prefabs/Avatar/Person 2.prefab",
                "Assets/_Project/Bundles/Prefabs/Avatar/Person 3.prefab",
            };

            public static string DefaultRoomAvatar = AvatarPaths[0];
            public static string DefaultRoomModel = ModelPaths[0];
        }
    }

    public static class Utils
    {
        public static T DeepCopyReflection<T>(T input, string[] exceptionFields = null)
        {
            var type = input.GetType();
            var properties = type.GetProperties();
            T clonedObj = (T)Activator.CreateInstance(type);
            foreach (var property in properties)
            {
                if (!exceptionFields.Contains(property.Name))
                    if (property.CanWrite)
                    {
                        object value = property.GetValue(input);
                        if (value != null && value.GetType().IsClass && !value.GetType().FullName.StartsWith("System."))
                        {
                            property.SetValue(clonedObj, DeepCopyReflection(value));
                        }
                        else
                        {
                            property.SetValue(clonedObj, value);
                        }
                    }
            }
            return clonedObj;
        }
    }
}
