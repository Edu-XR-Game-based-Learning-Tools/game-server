using System;
using System.Text;

namespace Core.Framework
{
    public static class PlayerPrefSerialization
    {
        public static string SerializeString(string sourceString)
        {
            byte[] sourceBytes = Encoding.ASCII.GetBytes(sourceString);
            return Convert.ToBase64String(sourceBytes);
        }

        public static string DeserializeString(string sourceString)
        {
            byte[] sourceBytes = Convert.FromBase64String(sourceString);
            return Encoding.ASCII.GetString(sourceBytes);
        }

        public static string SerializeFloat(float value)
        {
            // Convert the float into its 4 bytes
            byte[] bytes = BitConverter.GetBytes(value);

            // Represent those bytes as a base 64 string
            string base64 = Convert.ToBase64String(bytes);

            // Return the serialized version of that base 64 string
            return SerializeString(base64);
        }

        public static string SerializeInt(int value)
        {
            // Convert the int value into its 4 bytes
            byte[] bytes = BitConverter.GetBytes(value);

            // Represent those bytes as a base 64 string
            string base64 = Convert.ToBase64String(bytes);

            // Return the serialized version of that base 64 string
            return SerializeString(base64);
        }

        public static string SerializeBool(bool value)
        {
            // Convert the bool value into its 4 bytes
            byte[] bytes = BitConverter.GetBytes(value);

            // Represent those bytes as a base 64 string
            string base64 = Convert.ToBase64String(bytes);

            // Return the serialized version of that base 64 string
            return SerializeString(base64);
        }

        public static float DeserializeFloat(string sourceString)
        {
            // Deserialize the serialized string
            string decryptedString = DeserializeString(sourceString);

            // Convert the decrypted Base 64 representation back into bytes
            byte[] bytes = Convert.FromBase64String(decryptedString);

            // Turn the bytes back into a float and return it
            return BitConverter.ToSingle(bytes, 0);
        }

        public static int DeserializeInt(string sourceString)
        {
            // Deserialize the serialized string
            string decryptedString = DeserializeString(sourceString);

            // Convert the decrypted Base 64 representation back into bytes
            byte[] bytes = Convert.FromBase64String(decryptedString);

            // Turn the bytes back into a int and return it
            return BitConverter.ToInt32(bytes, 0);
        }

        public static bool DeserializeBool(string sourceString)
        {
            // Deserialize the serialized string
            string decryptedString = DeserializeString(sourceString);

            // Convert the decrypted Base 64 representation back into bytes
            byte[] bytes = Convert.FromBase64String(decryptedString);

            // Turn the bytes back into a bool and return it
            return BitConverter.ToBoolean(bytes, 0);
        }
    }
}