namespace OrganiMedCore.Email.Models
{
    public class TokenizedEmail
    {
        public string TemplateId { get; set; }

        public string Subject { get; set; }

        public string RawBody { get; set; }
    }
}
