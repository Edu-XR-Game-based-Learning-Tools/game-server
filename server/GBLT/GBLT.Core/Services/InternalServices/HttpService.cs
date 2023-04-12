using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Core.Service
{
    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public HttpService(ILogger<HttpService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

#nullable enable

        public async Task<T> HttpGet<T>(string uri, NameValueCollection? headers = null)
            where T : class
#nullable disable
        {
            PrepareHttpClient(headers);
            var result = await _httpClient.GetAsync(uri);
            if (!result.IsSuccessStatusCode)
                return null;

            return await FromHttpResponseMessage<T>(result);
        }

        public async Task<T> HttpDelete<T>(string uri)
            where T : class
        {
            PrepareHttpClient();
            var result = await _httpClient.DeleteAsync(uri);
            if (!result.IsSuccessStatusCode)
                return null;

            return await FromHttpResponseMessage<T>(result);
        }

        public async Task<T> HttpPost<T>(string uri, FormUrlEncodedContent payload, NameValueCollection headers = null)
            where T : class
        {
            PrepareHttpClient(headers);
            var result = await _httpClient.PostAsync(uri, payload);
            string res = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
                return null;

            return await FromHttpResponseMessage<T>(result);
        }

        public async Task<T> HttpPost<T>(string uri, object dataToSend, NameValueCollection headers = null)
            where T : class
        {
            PrepareHttpClient(headers);
            var payload = ToJson(dataToSend);
            var result = await _httpClient.PostAsync(uri, payload);
            string res = await result.Content.ReadAsStringAsync();
            _logger.LogError($"HttpPost {uri} {res}");
            return await FromHttpResponseMessage<T>(result);
        }

        public async Task<T> HttpPut<T>(string uri, object dataToSend)
            where T : class
        {
            PrepareHttpClient();
            var payload = ToJson(dataToSend);
            var result = await _httpClient.PutAsync(uri, payload);
            if (!result.IsSuccessStatusCode)
                return null;

            return await FromHttpResponseMessage<T>(result);
        }

        private StringContent ToJson(object obj)
        {
            return new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");
        }

        private async Task<T> FromHttpResponseMessage<T>(HttpResponseMessage result)
        {
            return JsonSerializer.Deserialize<T>(await result.Content.ReadAsStringAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false
            });
        }

#nullable enable

        private void PrepareHttpClient(NameValueCollection? headers = null)
#nullable disable
        {
            _httpClient.DefaultRequestHeaders.Clear();
            if (headers != null)
                foreach (var key in headers.AllKeys)
                    foreach (var value in headers.GetValues(key))
                        // _httpClient.DefaultRequestHeaders.Remove(key);
                        _httpClient.DefaultRequestHeaders.Add(key, value);
        }

        public string ToQueryString(NameValueCollection nvc)
        {
            string[] arrayParams =
            (
                from key in nvc.AllKeys
                from value in nvc.GetValues(key)
                select string.Format(
                    "{0}={1}",
                    HttpUtility.UrlEncode(key),
                    HttpUtility.UrlEncode(value))
            ).ToArray();

            return "?" + string.Join("&", arrayParams);
        }
    }
}