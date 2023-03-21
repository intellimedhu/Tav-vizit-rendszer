using IntelliMed.Core.Extensions;
using OrganiMedCore.Email.Settings;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.Email.ViewModels
{
    public class EmailSettingsViewModel
    {
        public bool Enabled { get; set; }

        public bool UseFakeEmails { get; set; }

        [Required]
        [Range(5, 50)]
        public int? EmailsDequeueLimit { get; set; }

        public string CcEmailAddresses { get; set; }

        public string BccEmailAddresses { get; set; }

        public string DebugEmailAddresses { get; set; }

        public string EmailFooter { get; set; }


        public void UpdateViewModel(EmailSettings model)
        {
            model.ThrowIfNull();

            Enabled = model.Enabled;
            UseFakeEmails = model.UseFakeEmails;
            EmailsDequeueLimit = model.EmailsDequeueLimit;
            CcEmailAddresses = string.Join(";", model.CcEmailAddresses);
            BccEmailAddresses = string.Join(";", model.BccEmailAddresses);
            DebugEmailAddresses = string.Join(";", model.DebugEmailAddresses);
            EmailFooter = model.EmailFooter;
        }
    }
}
