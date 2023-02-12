using System;

namespace Shared.Extension
{
    ///<summary>
    ///</summary>
    public static class UnixDateTimeHelper
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private const string InvalidUnixEpochErrorMessage = "Unix epoc starts January 1st, 1970";

        /// <summary>
        ///   Convert a long into a DateTime
        /// </summary>
        public static DateTime FromUnixTime(this Int64 self)
        {
            return UnixEpoch.AddSeconds(self);
        }

        public static DateTime FromUnixTime(this uint self)
        {
            return UnixEpoch.AddSeconds(self);
        }

        public static DateTime FromUnixTime(this int self)
        {
            return UnixEpoch.AddSeconds(self);
        }

        /// <summary>
        ///   Convert a long of Milliseconds into a DateTime
        /// </summary>
        public static DateTime FromUnixTimeMs(this Int64 self)
        {
            return UnixEpoch.AddMilliseconds(self);
        }

        /// <summary>
        ///   Convert a DateTime into a long
        /// </summary>
        public static long ToUnixTime(this DateTime self)
        {
            if (self < UnixEpoch)
            {
                if (self == DateTime.MinValue)
                {
                    return 0;
                }

                throw new ArgumentOutOfRangeException(InvalidUnixEpochErrorMessage);
            }

            TimeSpan delta = self.Subtract(UnixEpoch);
            var result = (long)delta.TotalSeconds;
            return result;
        }

        /// <summary>
        ///   Convert a DateTime into a long of milliseconds
        /// </summary>
        public static long ToUnixTimeMs(this DateTime self)
        {
            if (self < UnixEpoch)
            {
                if (self == DateTime.MinValue)
                {
                    return 0;
                }

                throw new ArgumentOutOfRangeException(InvalidUnixEpochErrorMessage);
            }

            TimeSpan delta = self.Subtract(UnixEpoch);
            var result = (long)delta.TotalMilliseconds;
            return result;
        }

        /// <summary>
        ///   Converts a long of milliseconds to an ISO 8061 Datetime string, used for Appboy
        /// </summary>
        public static string ToIso8601Date(this DateTime self)
        {
            return self.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}