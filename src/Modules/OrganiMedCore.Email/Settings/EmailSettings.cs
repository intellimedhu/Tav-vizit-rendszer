using System.Collections.Generic;

namespace OrganiMedCore.Email.Settings
{
    public class EmailSettings
    {
        public bool Enabled { get; set; }

        public bool UseFakeEmails { get; set; }

        public int EmailsDequeueLimit { get; set; }

        public HashSet<string> CcEmailAddresses { get; set; } = new HashSet<string>();

        public HashSet<string> BccEmailAddresses { get; set; } = new HashSet<string>();

        public HashSet<string> DebugEmailAddresses { get; set; } = new HashSet<string>();

        public string EmailFooter { get; set; }
    }
}
