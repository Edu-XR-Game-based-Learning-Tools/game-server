using Core.Entity;
using Shared.Network;
using System;
using System.Globalization;

namespace Core.Utility
{
    public static class DateTimeExtension
    {
        public static DateTime ToDateTime(this double self)
        {
            DateTime dtDateTime = new(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(self).ToLocalTime();
            return dtDateTime;
        }

        public static DateTime? ToDateTime(this string self)
        {
            if (DateTime.TryParse(self, out DateTime temp))
                return temp;
            return null;
        }

        public static double GetUnixTimeStamp()
        {
            TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            return timeSpan.TotalSeconds;
        }

        public static double ToUnixTimeStamp(this string dateString)
        {
            if (DateTime.TryParse(dateString, out DateTime temp))
            {
                TimeSpan timeSpan = temp - new DateTime(1970, 1, 1, 0, 0, 0);
                return timeSpan.TotalSeconds;
            }
            return 0;
        }

        public static long ToUnixTimeSeconds(this DateTime self)
        {
            return ((DateTimeOffset)self).ToUnixTimeSeconds();
        }

        public static long ToUnixTimeSeconds(this DateTime? self)
        {
            return self != null ? ((DateTimeOffset)self).ToUnixTimeSeconds() : 0;
        }
    }

    public static class StringExtension
    {
        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static float ParseFloat(this string self)
        {
            self = self.Replace(',', '.');
            return float.Parse(self, CultureInfo.InvariantCulture.NumberFormat);
        }

        public static float? TryParseFloat(this string self)
        {
            self = self.Replace(',', '.');
            if (float.TryParse(self, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out float result))
                return result;

            return null;
        }
    }

    public static class FloatExtension
    {
    }
}