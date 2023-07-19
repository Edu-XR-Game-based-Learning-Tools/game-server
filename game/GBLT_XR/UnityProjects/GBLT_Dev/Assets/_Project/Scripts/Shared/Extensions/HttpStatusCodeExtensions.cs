using System.Net;

namespace Shared.Extension
{
    /// <summary>
    /// Classify Http status codes into our more generic groups
    /// Keep this in sync with the HttpExtensions methods, in particular the RPC to HTTP error code conversions
    /// https://msdn.microsoft.com/en-us/library/system.net.httpstatuscode(v=vs.110).aspx
    /// </summary>
    public static class HttpStatusCodeExtensions
    {
        public static HttpWebResponse TryGetWebResponse(WebException e)
        {
            if (e != null)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    return e.Response as HttpWebResponse;
                }
            }

            return null;
        }

        public static bool IsSuccess(this HttpStatusCode httpStatus)
        {
            int status = (int)httpStatus;
            return status > 199 && status < 300;
        }

        public static bool IsNetworkUnreachable(this HttpStatusCode code)
        {
            switch (code)
            {
                case HttpStatusCode.ProxyAuthenticationRequired:
                    return true;
            }

            return false;
        }

        public static bool IsConcurrencyError(this HttpStatusCode code)
        {
            switch (code)
            {
                case HttpStatusCode.Conflict:
                    return true;
            }

            return false;
        }

        public static bool IsServerInternalError(this HttpStatusCode code)
        {
            switch (code)
            {
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.InternalServerError:
                case (HttpStatusCode)429:
                    return true;
            }

            return false;
        }

        public static bool IsServerUnreachable(this HttpStatusCode code)
        {
            switch (code)
            {
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.Gone:
                case HttpStatusCode.Moved:
                case HttpStatusCode.NotFound:
                case HttpStatusCode.NotImplemented:
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.BadGateway:
                    return true;
            }

            return false;
        }

        public static bool IsTimeout(this HttpStatusCode code)
        {
            switch (code)
            {
                case HttpStatusCode.GatewayTimeout:
                case HttpStatusCode.RequestTimeout:
                    return true;
            }

            return false;
        }
    }
}