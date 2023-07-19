using MessagePack;

namespace Core.Utility
{
    public static class SerializerExtension
    {
        public static byte[] Serialize<T>(this T self)
        {
            return MessagePackSerializer.Serialize(self);
        }

        public static T Deserialize<T>(this byte[] self)
        {
            return MessagePackSerializer.Deserialize<T>(self);
        }
    }
}