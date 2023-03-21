using OrganiMedCore.UriAuthentication.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.UriAuthentication.Services
{
    public interface INonceService
    {
        Task CreateAsync(Nonce nonce);

        Task CreateManyAsync(IEnumerable<Nonce> nonces);

        Task<Nonce> GetByValue(Guid value);

        Task<string> GetUriAsync(Guid value);

        Task CleanupAsync();
    }
}
