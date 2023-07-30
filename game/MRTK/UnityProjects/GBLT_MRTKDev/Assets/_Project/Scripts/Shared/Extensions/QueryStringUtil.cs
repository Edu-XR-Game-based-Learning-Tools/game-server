using System.Collections.Specialized;

namespace Shared.Extension
{
    public static class QueryStringUtil
    {
        public static NameValueCollection ParseQueryString(string queryString)
        {
            if (string.IsNullOrEmpty(queryString))
            {
                return new NameValueCollection();
            }

            var queryStringNameValues = new NameValueCollection();

            int indexOfQuerySeperator = queryString.IndexOf('?');
            if (indexOfQuerySeperator > -1)
            {
                if (queryString.Length >= indexOfQuerySeperator + 1)
                {
                    queryString = queryString.Substring(indexOfQuerySeperator + 1);
                }
            }

            string[] pairs = queryString.Split('&');
            for (int i = 0; i < pairs.Length; i++)
            {
                string[] parts = pairs[i].Split('=');
                queryStringNameValues.Add(parts[0], parts.Length > 1 ? parts[1] : string.Empty);
            }

            return queryStringNameValues;
        }
    }
}