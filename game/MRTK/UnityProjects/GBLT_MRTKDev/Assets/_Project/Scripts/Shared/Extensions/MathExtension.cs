using System;

namespace Shared.Extension
{
    public static class MathExtension
    {
        public static bool IsBetweenRange(this float thisValue, float lesser, float greater)
        {
            return thisValue >= lesser && thisValue <= greater;
        }

        public static float[] ConvertByteToFloat(this byte[] byteArray)
        {
            int len = byteArray.Length / 4;
            float[] floatArray = new float[len];
            for (int i = 0; i < byteArray.Length; i += 4)
            {
                floatArray[i / 4] = BitConverter.ToSingle(byteArray, i);
            }
            return floatArray;
        }

        public static byte[] ConvertFloatToByte(this float[] floatArray)
        {
            int len = floatArray.Length * 4;
            byte[] byteArray = new byte[len];
            int pos = 0;
            foreach (float f in floatArray)
            {
                byte[] data = BitConverter.GetBytes(f);
                Array.Copy(data, 0, byteArray, pos, 4);
                pos += 4;
            }
            return byteArray;
        }
    }
}
