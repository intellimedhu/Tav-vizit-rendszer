using System;
using System.Collections.Generic;

namespace OrganiMedCore.Email.Models
{
    public class EmailTemplate
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public HashSet<string> Tokens { get; set; } = new HashSet<string>();
    }
}
