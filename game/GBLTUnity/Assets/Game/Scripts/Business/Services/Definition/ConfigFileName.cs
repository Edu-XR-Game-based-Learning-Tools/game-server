using System;
using System.Collections.Generic;

namespace Core.Business {

    public static class ConfigFileName {
        public static string Suffix = ".json";

        public static Dictionary<Type, string> Mapper = new Dictionary<Type, string>()
        {
            { typeof(GeneralConfigDefinition), "GeneralConfig" },
        };

        public static string GetFileName<T>() {
            return Mapper[typeof(T)] + Suffix;
        }
    }
}