using IntelliMed.Core.Extensions;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrganiMedCore.Email.Constants;
using OrganiMedCore.Email.Settings;
using OrganiMedCore.Email.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.Email.Drivers
{
    public class EmailSettingsDisplayDriver : SectionDisplayDriver<ISite, EmailSettings>
    {
        public IStringLocalizer T { get; set; }


        public EmailSettingsDisplayDriver(IStringLocalizer<EmailSettingsDisplayDriver> stringLocalizer)
        {
            T = stringLocalizer;
        }


        public override IDisplayResult Edit(EmailSettings section, BuildEditorContext context)
        {
            Prefix = string.Empty;

            return Initialize<EmailSettingsViewModel>("EmailSettings_Edit", viewModel => viewModel.UpdateViewModel(section))
                .Location("Content:1")
                .OnGroup(GroupIds.EmailTemplateSettings);
        }

        public override async Task<IDisplayResult> UpdateAsync(EmailSettings section, BuildEditorContext context)
        {
            if (context.GroupId == GroupIds.EmailTemplateSettings)
            {
                var viewModel = new EmailSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(viewModel);

                if (viewModel.EmailsDequeueLimit.HasValue)
                {
                    section.EmailsDequeueLimit = viewModel.EmailsDequeueLimit.Value;
                }

                ValidateEmails(viewModel.BccEmailAddresses, context, out var emailAddresses);
                section.BccEmailAddresses = emailAddresses;

                ValidateEmails(viewModel.CcEmailAddresses, context, out emailAddresses);
                section.CcEmailAddresses = emailAddresses;

                ValidateEmails(viewModel.DebugEmailAddresses, context, out emailAddresses);
                section.DebugEmailAddresses = emailAddresses;

                section.Enabled = viewModel.Enabled;
                section.UseFakeEmails = viewModel.UseFakeEmails;

                section.EmailFooter = viewModel.EmailFooter;
            }

            return await EditAsync(section, context);
        }


        private void ValidateEmails(string notificationEmailAddresses, BuildEditorContext context, out HashSet<string> emailAddresses)
        {
            emailAddresses = new HashSet<string>();

            if (!string.IsNullOrEmpty(notificationEmailAddresses))
            {
                var emails = new HashSet<string>(notificationEmailAddresses
                    ?.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(email => email.Trim().ToLower()));

                foreach (var email in emails)
                {
                    if (!email.IsEmail())
                    {
                        context.Updater.ModelState.AddModelError(
                            string.Empty,
                            T["Helytelen email cím: {0}", email]);
                    }
                }

                emailAddresses = emails;
            }
        }
    }
}
