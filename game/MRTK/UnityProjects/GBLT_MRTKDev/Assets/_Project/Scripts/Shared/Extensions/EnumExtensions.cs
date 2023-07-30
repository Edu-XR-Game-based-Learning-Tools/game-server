using System;

namespace Shared.Extension
{
    public static class EnumExtensions
    {
        public static TTo ParseEnumToEnum<TFrom, TTo>(this TFrom from) where TFrom : Enum
        {
            return (TTo)Enum.Parse(typeof(TTo), from.ToString());
        }
    }
}