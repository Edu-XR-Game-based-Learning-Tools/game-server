using MemoryPack;

namespace Core.Utility
{
    public static class SerializerExtension
    {
        public static byte[] Serialize<T>(this T self)
        {
            return MemoryPackSerializer.Serialize(self);
        }

        public static T Deserialize<T>(this byte[] self)
        {
            return MemoryPackSerializer.Deserialize<T>(self);
        }
    }
}