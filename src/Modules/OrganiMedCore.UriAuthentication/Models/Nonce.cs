using System;

namespace OrganiMedCore.UriAuthentication.Models
{
    public class Nonce
    {
        public Guid Value { get; set; }

        public int TypeId { get; set; }

        public NonceType Type { get; set; }

        public string RedirectUrl { get; set; }

        public DateTime? ExpirationDate { get; set; }
    }
}
