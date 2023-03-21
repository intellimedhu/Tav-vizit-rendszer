using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace IntelliMed.Core.Services
{
    public class KeepAliveService
    {
        public async Task<bool> KeepAliveAsync(Uri requestUri)
        {
            using (var request = new HttpRequestMessage()
            {
                RequestUri = requestUri,
                Method = HttpMethod.Head
            })
            {
                using (var client = new HttpClient())
                {
                    using (var response = await client.SendAsync(request))
                    {
                        return response.IsSuccessStatusCode;
                    }
                }
            }
        }
    }
}
