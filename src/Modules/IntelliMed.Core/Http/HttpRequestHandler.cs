using IntelliMed.Core.Extensions;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMed.Core.Http
{
    public class HttpRequestHandler : IHttpRequestHandler
    {
        private readonly HttpClient _httpClient;


        public HttpRequestHandler(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<HttpResponseMessage> SendRequestAsync(HttpRequestContext context)
        {
            context.ThrowIfNull();
            context.RequestUri.ThrowIfNull();
            context.Method.ThrowIfNull();
            if (context.Content != null)
            {
                context.MediaType.ThrowIfNull();
            }

            using (var request = new HttpRequestMessage())
            {
                request.RequestUri = context.RequestUri;
                request.Method = context.Method;

                if (!string.IsNullOrEmpty(context.AuthorizationScheme) && !string.IsNullOrEmpty(context.AuthorizationParameters))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue(context.AuthorizationScheme, context.AuthorizationParameters);
                }

                if (context.Content != null)
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(context.Content), Encoding.UTF8, context.MediaType);
                }

                return await _httpClient.SendAsync(request);
            }
        }

        public async Task<T> SendRequestAsync<T>(HttpRequestContext context) where T : new()
        {
            using (var response = await SendRequestAsync(context))
            {
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsAsync<T>();
            }
        }
    }
}
