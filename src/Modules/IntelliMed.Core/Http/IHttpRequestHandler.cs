using System.Net.Http;
using System.Threading.Tasks;

namespace IntelliMed.Core.Http
{
    public interface IHttpRequestHandler
    {
        Task<HttpResponseMessage> SendRequestAsync(HttpRequestContext context);

        Task<T> SendRequestAsync<T>(HttpRequestContext context) where T : new();
    }
}
