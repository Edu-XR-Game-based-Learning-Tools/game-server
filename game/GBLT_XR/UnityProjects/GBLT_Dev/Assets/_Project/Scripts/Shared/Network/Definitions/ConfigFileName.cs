using System;
using System.Collections.Generic;

namespace Shared.Network
{
    public static class ConfigFileName
    {
        private static readonly string _suffix = ".json";
        private static readonly Dictionary<Type, string> _mapper = new()
        {
            { typeof(GeneralConfigDefinition), "GeneralConfig" },
            { typeof(ClassRoomDefinition), "ClassRoomDefinition" },
        };

        public static string GetFileName<T>()
        {
            return _mapper[typeof(T)] + _suffix;
        }
    }
}
