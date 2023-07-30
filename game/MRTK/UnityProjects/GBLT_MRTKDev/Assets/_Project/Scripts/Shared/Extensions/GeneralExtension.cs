using System;
using System.Reflection;

namespace Shared.Extension
{
    public static class GeneralExtension
    {
        public static bool HasMethod(this object objectToCheck, string methodName)
        {
            var type = objectToCheck.GetType();
            return type.GetMethod(methodName) != null;
        }

        public static void CallMethod(this object obj, string methodName, params object[] args)
        {
            Type thisType = obj.GetType();
            MethodInfo theMethod = thisType.GetMethod(methodName);
            theMethod.Invoke(obj, args);
        }

        public static object CreateInstance(Type type)
        {
            object instace = Activator.CreateInstance(type);
            return instace;
        }

        public static string RandomName()
        {
            Random random = new();
            string name = "User" + random.Next(0, 1000000);
            return name;
        }
    }
}