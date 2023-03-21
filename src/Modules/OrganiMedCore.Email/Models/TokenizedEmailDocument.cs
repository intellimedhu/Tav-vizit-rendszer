using System.Collections.Generic;

namespace OrganiMedCore.Email.Models
{
    public class TokenizedEmailDocument
    {
        public int Id { get; set; }

        public IList<TokenizedEmail> TokenizedEmails { get; set; } = new List<TokenizedEmail>();
    }
}
