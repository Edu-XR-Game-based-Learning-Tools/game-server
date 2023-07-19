using System;
using System.Collections.Generic;
using System.Linq;

namespace Shared.Extension
{
    public static class StringExtensions
    {
        /// <summary>
        /// Convert a list of objects into a lexical (comma-separated) list as a string.
        /// </summary>
        /// <param name="useOxfordComma">if <c>true</c>, a comma will appear between the penultimate and ultimate items of the list when there are three or more items.</param>
        public static string ToLexicalList<T>(this IEnumerable<T> self, bool useOxfordComma = true)
        {
            string msg;
            var nameItems = self.Select(i => i == null ? "null" : i.ToString()).ToArray();
            switch (nameItems.Length)
            {
                case 0:
                    msg = null;
                    break;

                case 1:
                    msg = nameItems[0];
                    break;

                case 2:
                    msg = nameItems[0] + " and " + nameItems[1];
                    break;

                default:
                    msg = nameItems[0];
                    for (int i = 1; i < nameItems.Length - 1; ++i)
                        msg = msg + ", " + nameItems[i];
                    if (useOxfordComma)
                        msg = msg + ", and " + nameItems[nameItems.Length - 1];
                    else
                        msg = msg + " and " + nameItems[nameItems.Length - 1];
                    break;
            }
            return msg;
        }

        /// <summary>
        /// Turn a collection of objects into a collection of strings via ToString().  <c>null</c> input values remain nulls in the output.
        /// </summary>
        /// <param name="omitNullsAndEmptyStrings">if <c>true</c>, empty strings and <c>null</c> values will not appear as elements in the output.</param>
        public static IEnumerable<string> ToStrings<T>(this IEnumerable<T> self, bool omitNullsAndEmptyStrings = true)
        {
            var nameItems = self.Select(i => i == null ? null : i.ToString());
            if (omitNullsAndEmptyStrings)
            {
                nameItems = nameItems.WhereNot(String.IsNullOrEmpty);
            }
            return nameItems;
        }

        /// <summary>
        /// Join a collection of objects into a single string.
        /// </summary>
        /// <param name="separator">a string to place between each item</param>
        /// <param name="omitNullsAndEmptyStrings">if <c>true</c>, empty strings and <c>null</c> values will not appear as elements in the output.</param>
        public static string Join<T>(this IEnumerable<T> self, string separator = ", ", bool omitNullsAndEmptyStrings = true)
        {
            var nameItems = self.ToStrings(omitNullsAndEmptyStrings);
            return String.Join(separator, nameItems.ToArray());
        }

        /// <summary>
        /// Returns a string with the first letter capitalized.
        /// </summary>
        public static string UpperFirst(this string self)
        {
            if (self == null || self.Length == 0)
            {
                return self;
            }
            return Char.ToUpper(self[0]) + self.Substring(1);
        }

        /// <summary>
        /// Returns a substring of the given string from the last occurence of <paramref name="left"/> to the
        /// first occurance of <paramref name="right"/> after that, thus:
        ///
        /// "http://www.foo.com/path/to/file.html?cgi_arg=baz".Slice('/', '?') == "file.html"
        /// "http://www.foo.com/path/to/file.html".Slice('/', '?') == "file.html"
        /// "something".Slice('/', '?') == "something"
        ///
        /// If either character isn't present, the beginning or end of the string is used.
        /// </summary>
        public static string Slice(this string self, char left, char right)
        {
            return self.Split(left).Last().Split(right).First();
        }

        public static string FormatWith(this string self, params object[] args)
        {
            try
            {
                return String.Format(self, args);
            }
            catch (Exception e)
            {
                return e.GetType().Name + " in [" + self + "]:" + e.Message;
            }
        }

        public static string UnderscoreCase(this string self)
        {
            if (string.IsNullOrEmpty(self))
            {
                return self;
            }

            string result = char.ToLowerInvariant(self[0]).ToString();
            for (int i = 1; i < self.Length; i++)
            {
                if (char.IsUpper(self[i]))
                {
                    result += "_" + char.ToLowerInvariant(self[i]);
                }
                else
                {
                    result += self[i];
                }
            }

            return result;
        }

        public static string LowerAndUnderscoreSpace(this string self)
        {
            return self.ToLowerInvariant().Replace(" ", "_");
        }
    }
}