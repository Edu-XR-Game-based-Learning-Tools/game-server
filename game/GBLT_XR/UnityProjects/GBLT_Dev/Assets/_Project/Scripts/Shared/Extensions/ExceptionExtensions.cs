using System;
using System.Net;
using System.Net.Sockets;

namespace Shared.Extension
{
    public static class ExceptionExtensions
    {
        public static bool ShouldRetryRequest(this Exception e)
        {
            return e.IsTimeout() ||
                   e.IsDnsTimeout() ||
                   e.IsNetworkUnreachable() ||
                   e.IsSSLCertError() ||
                   e.IsUnknownWebException();
        }

        public static string GetFailureReason(this Exception e)
        {
            if (e.IsTimeout())
            {
                return "Timeout";
            }

            if (e.IsDnsTimeout())
            {
                return "DNS Timeout";
            }

            if (e.IsNetworkUnreachable())
            {
                return "Network Unreachable";
            }

            if (e.IsSSLCertError())
            {
                return "SSL Cert";
            }

            return "Unknown Network Error";
        }

        public static bool IsSSLCertError(this Exception e)
        {
            if (e == null)
            {
                return false;
            }

            //if (ExceptionErrorMessageParser.IsErrorASSLCertError(e.Message))
            //{
            //    return true;
            //}

            if (e.InnerException != null)
            {
                return e.InnerException.IsSSLCertError();
            }

            return false;
        }

        public static bool IsDnsTimeout(this Exception e)
        {
            if (e == null)
            {
                return false;
            }

            var dnsException = e as DnsTimeoutException;
            if (dnsException != null)
            {
                return true;
            }

            var webException = e as System.Net.WebException;
            if (webException != null)
            {
                switch (webException.Status)
                {
                    case WebExceptionStatus.NameResolutionFailure:
                    case WebExceptionStatus.ConnectFailure:
                    case WebExceptionStatus.ProxyNameResolutionFailure:
                        return true;
                }
            }

            var socketException = e as SocketException;
            if (socketException != null)
            {
                switch (socketException.SocketErrorCode)
                {
                    case SocketError.HostNotFound:
                        return true;
                }
            }

            //if (ExceptionErrorMessageParser.IsErrorADnsTimeout(e.Message))
            //{
            //    return true;
            //}

            if (e.InnerException != null)
            {
                return e.InnerException.IsDnsTimeout();
            }

            return false;
        }

        public static bool IsTimeout(this Exception e)
        {
            if (e == null)
            {
                return false;
            }

            var customTimeoutException = e as TimeoutException;
            if (customTimeoutException != null)
            {
                return true;
            }

            var timeoutException = e as System.TimeoutException;
            if (timeoutException != null)
            {
                return true;
            }

            var webException = e as System.Net.WebException;
            if (webException != null)
            {
                HttpWebResponse httpWebResponse = HttpStatusCodeExtensions.TryGetWebResponse(webException);
                if (httpWebResponse != null)
                {
                    if (httpWebResponse.StatusCode.IsTimeout())
                    {
                        return true;
                    }
                }

                switch (webException.Status)
                {
                    case WebExceptionStatus.RequestCanceled:
                    case WebExceptionStatus.KeepAliveFailure:
                    case WebExceptionStatus.Timeout:
                        return true;

                    case WebExceptionStatus.UnknownError:
                        // In some cases the request aborts and timeouts yield status UnknownError
                        return (webException.Response == null);
                }
            }

            var socketError = e as SocketException;
            if (socketError != null)
            {
                if (socketError.SocketErrorCode == SocketError.TimedOut)
                {
                    return true;
                }
            }

            //if (ExceptionErrorMessageParser.IsErrorAConnectionTimeout(e.Message))
            //{
            //    return true;
            //}

            if (e.InnerException != null)
            {
                return e.InnerException.IsTimeout();
            }

            return false;
        }

        public static bool IsUnknownWebException(this Exception e)
        {
            if (e == null)
            {
                return false;
            }

            var customWebException = e as WebException;
            if (customWebException != null
                && customWebException.Status == WebExceptionStatus.UnknownError
                && !Enum.IsDefined(typeof(HttpStatusCode), customWebException.StatusCode))
            {
                // There are bugs with UnityWebRequest where it can return a status code of 0,
                // which is not a valid HttpStatusCode
                return true;
            }

            //if (ExceptionErrorMessageParser.IsErrorAnUnknownWebException(e.Message))
            //{
            //    return true;
            //}

            if (e.InnerException != null)
            {
                return e.InnerException.IsUnknownWebException();
            }

            return false;
        }

        public static bool IsNetworkUnreachable(this Exception e)
        {
            if (e == null)
            {
                return false;
            }

            var unreachableException = e as NetworkUnreachableException;
            if (unreachableException != null)
            {
                return true;
            }

            var webException = e as System.Net.WebException;
            if (webException != null)
            {
                HttpWebResponse httpWebResponse = HttpStatusCodeExtensions.TryGetWebResponse(webException);
                if (httpWebResponse != null)
                {
                    if (httpWebResponse.StatusCode.IsNetworkUnreachable())
                    {
                        return true;
                    }
                }
            }

            var socketException = e as SocketException;
            if (socketException != null)
            {
                switch (socketException.SocketErrorCode)
                {
                    case SocketError.NetworkDown:
                    case SocketError.NetworkUnreachable:
                    case SocketError.ConnectionAborted:
                    case SocketError.HostUnreachable:
                        // This indicates an error with the client's connection OR
                        // reaching the server in the first place.
                        // We cannot do much about this.
                        return true;
                }
            }

            //if (ExceptionErrorMessageParser.IsErrorNetworkUnreachable(e.Message))
            //{
            //    return true;
            //}

            if (e.InnerException != null)
            {
                return e.InnerException.IsNetworkUnreachable();
            }

            return false;
        }

        public static bool IsServerUnreachable(this Exception e)
        {
            if (e == null)
            {
                return false;
            }

            var webException = e as System.Net.WebException;
            if (webException != null)
            {
                HttpWebResponse httpWebResponse = HttpStatusCodeExtensions.TryGetWebResponse(webException);
                if (httpWebResponse != null)
                {
                    if (httpWebResponse.StatusCode.IsServerUnreachable())
                    {
                        return true;
                    }
                }
            }

            var socketException = e as SocketException;
            if (socketException != null)
            {
                switch (socketException.SocketErrorCode)
                {
                    case SocketError.HostDown:
                    case SocketError.ConnectionRefused:
                    case SocketError.ConnectionReset:
                        // The server likely had an error and stuck a knife in our connection
                        return true;
                }
            }

            if (e.InnerException != null)
            {
                return e.InnerException.IsServerUnreachable();
            }

            return false;
        }

        public class DnsTimeoutException : Exception
        {
            public DnsTimeoutException(string message)
                : base(message)
            {
            }
        }

        public class TimeoutException : Exception
        {
            public TimeoutException(string message)
                : base(message)
            {
            }
        }

        public class NetworkUnreachableException : Exception
        {
            public NetworkUnreachableException(string message)
                : base(message)
            {
            }
        }

        public class WebException : System.Net.WebException
        {
            private readonly HttpStatusCode _statusCode;

            public HttpStatusCode StatusCode
            {
                get { return _statusCode; }
            }

            public WebException(
                string message,
                Exception innerException,
                WebExceptionStatus status,
                WebResponse response,
                HttpStatusCode statusCode)
                    : base(message, innerException, status, response)
            {
                _statusCode = statusCode;
            }
        }
    }
}