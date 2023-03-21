using System.Collections.Generic;

namespace OrganiMedCore.UriAuthentication.Models
{
    public class NonceDocument
    {
        public int Id { get; set; }

        public List<Nonce> Nonces { get; set; } = new List<Nonce>();
    }
}
