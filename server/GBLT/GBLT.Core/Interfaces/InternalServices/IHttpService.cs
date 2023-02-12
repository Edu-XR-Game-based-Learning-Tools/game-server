using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;

namespace Core.Service
{
    public interface IHttpService
    {
#nullable enable

        Task<T> HttpGet<T>(string uri, NameValueCollection? headers = null)
            where T : class;

#nullable disable

        Task<T> HttpDelete<T>(string uri)
            where T : class;

        Task<T> HttpPost<T>(string uri, FormUrlEncodedContent payload, NameValueCollection headers = null)
            where T : class;

        Task<T> HttpPost<T>(string uri, object dataToSend, NameValueCollection headers = null)
            where T : class;

        Task<T> HttpPut<T>(string uri, object dataToSend)
            where T : class;

        string ToQueryString(NameValueCollection nvc);
    }
}