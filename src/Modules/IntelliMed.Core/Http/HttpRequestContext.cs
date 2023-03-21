using System;
using System.Net.Http;

namespace IntelliMed.Core.Http
{
    public class HttpRequestContext
    {
        public Uri RequestUri { get; set; }

        public string AuthorizationScheme { get; set; }

        public string AuthorizationParameters { get; set; }

        public HttpMethod Method { get; set; } = HttpMethod.Get;

        public object Content { get; set; }

        public string MediaType { get; set; } = "application/json";
    }
}
