using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Shared.Extension
{
    public static class NameValueCollectionExtensions
    {
        /// <summary>
        /// Returns a string with the first letter capitalized.
        /// </summary>
        public static IDictionary<string, string> ToDictionary(this NameValueCollection nameValueCollection)
        {
            if (nameValueCollection == null || nameValueCollection.Count == 0)
            {
                return new Dictionary<string, string>();
            }

            IDictionary<string, string> dictionary = new Dictionary<string, string>();

            foreach (string key in nameValueCollection.AllKeys)
            {
                dictionary.Add(key, nameValueCollection[key]);
            }
            return dictionary;
        }

        public static string ToQueryString(this NameValueCollection nvc)
        {
            string[] arrayParams =
            (
                from key in nvc.AllKeys
                from value in nvc.GetValues(key)
                select string.Format(
                    "{0}={1}",
                    key,
                    value)
            ).ToArray();

            return "?" + string.Join("&", arrayParams);
        }
    }
}