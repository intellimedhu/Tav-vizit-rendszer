using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.Email.Models;
using OrganiMedCore.Email.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public class EmailTemplateProvider : EmailTemplateProviderBase
    {
        private readonly IEnumerable<EmailTemplate> _emailTemplates = new[]
        {
            new EmailTemplate()
            {
                Id = EmailTemplateIds.CenterProfileAccepted,
                Name = "Szakellátóhely adatlapja elfogadásra került - szintenként lefelé mindenkinek",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileAcceptedViewModel.AccreditationStatus),
                    nameof(CenterProfileAcceptedViewModel.CenterName),
                    nameof(CenterProfileAcceptedViewModel.CurrentRole),
                    nameof(CenterProfileAcceptedViewModel.PersonName)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.CenterProfileAssignedOnTenant,
                Name = "Szakellátóhely technikai elkészültéről - vezetőnek",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileAssignedOnTenantViewModel.CenterName),
                    nameof(CenterProfileAssignedOnTenantViewModel.LeaderName),
                    nameof(CenterProfileAssignedOnTenantViewModel.Nonce)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.CenterProfileCreated,
                Name = "Szakellátóhely létrehozási kérelem benyújtva - adminnak",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileCreatedTemplateViewModel.CenterName),
                    nameof(CenterProfileCreatedTemplateViewModel.LeaderName)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.CenterProfileCreatedFeedback,
                Name = "Szakellátóhely létrehozási kérelem benyújtva - visszaigazolás a vezetőnek",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileCreatedTemplateViewModel.CenterName),
                    nameof(CenterProfileCreatedTemplateViewModel.LeaderName)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.CenterProfileLeaderChanged,
                Name = "Szakellátóhely vezető leváltása - új vezetőnek",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileLeaderChanged.CenterName),
                    nameof(CenterProfileLeaderChanged.Nonce),
                    nameof(CenterProfileLeaderChanged.NewLeaderName)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.CenterProfileLeaderChangedBeforeAssignment,
                Name = "Szakellátóhely vezető leváltása, szakellátóhely intézményhez rendelés előtt - adminnak",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileLeaderChanged.CenterName),
                    nameof(CenterProfileLeaderChanged.NewLeaderName)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.CenterProfileRejected,
                Name = "Szakellátóhely adatlap visszautasítása - vezetőnek, [TR-nek], [MDT-OMKB-nak]",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileRejectedViewModel.CenterName),
                    nameof(CenterProfileRejectedViewModel.CurrentRole),
                    nameof(CenterProfileRejectedViewModel.PersonName),
                    nameof(CenterProfileRejectedViewModel.RejectReason),
                    nameof(CenterProfileRejectedViewModel.Nonce)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.CenterProfileSubmission,
                Name = "Szakellátóhely adatlap bejelentése - TR-nek",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileSubmissionViewModel.CenterLeaderName),
                    nameof(CenterProfileSubmissionViewModel.CenterName),
                    nameof(CenterProfileSubmissionViewModel.ReviewerMemberFullName),
                    nameof(CenterProfileSubmissionViewModel.Nonce)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.CenterProfileSubmissionFeedback,
                Name = "Szakellátóhely adatlap bejelentése - visszaigazolás a vezetőnek",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileSubmissionFeedbackViewModel.CenterLeaderName),
                    nameof(CenterProfileSubmissionFeedbackViewModel.CenterName),
                    nameof(CenterProfileSubmissionFeedbackViewModel.Reviewers)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.ColleagueAction_ApplicationCancelled,
                Name = "Jelentkezés visszavonása esetén - Szakellátóhely vezetőnek",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileColleagueActionNotificationViewModel.LeaderName),
                    nameof(CenterProfileColleagueActionNotificationViewModel.CenterName),
                    nameof(CenterProfileColleagueActionNotificationViewModel.ColleagueName),
                    nameof(CenterProfileColleagueActionNotificationViewModel.Occupation),
                    nameof(CenterProfileColleagueActionNotificationViewModel.Nonce)
                }
            },
            new EmailTemplate()
            {
                Id =  EmailTemplateIds.ColleagueAction_ApplicationSubmitted,
                Name = "Jelentkezés szakellátóhelyre munkatárs részéről - Szakellátóhely vezetőnek",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileColleagueActionNotificationViewModel.LeaderName),
                    nameof(CenterProfileColleagueActionNotificationViewModel.CenterName),
                    nameof(CenterProfileColleagueActionNotificationViewModel.ColleagueName),
                    nameof(CenterProfileColleagueActionNotificationViewModel.Occupation),
                    nameof(CenterProfileColleagueActionNotificationViewModel.Nonce)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.ColleagueAction_ByLeader_AcceptApplication,
                Name = "Szakellátóhely vezető elfogadja a jelentkezést - munkatársnak",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.LeaderName),
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.CenterName),
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.ColleagueName),
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.Occupation),
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.Nonce)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.ColleagueAction_ByLeader_CancelInvitation_ToMember,
                Name = "Szakellátóhely vezető visszavonja a meghívást - munkatársnak, aki tag",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.LeaderName),
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.CenterName),
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.ColleagueName),
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.Occupation),
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.Nonce)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.ColleagueAction_ByLeader_CancelInvitation_ToNonMember,
                Name = "Szakellátóhely vezető visszavonja a meghívást - munkatársnak, aki nem tag",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileColleagueActionNotificationViewModel.LeaderName),
                    nameof(CenterProfileColleagueActionNotificationViewModel.CenterName),
                    nameof(CenterProfileColleagueActionNotificationViewModel.ColleagueName)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.ColleagueAction_ByLeader_ColleagueInvitation_ToMember,
                Name = "Szakellátóhely vezető meghív egy munkatársat - munkatársnak, aki tag",
                Tokens = new HashSet<string>()
                {
                    nameof(ColleagueInvitationByLeaderToMemberViewModel.CenterName),
                    nameof(ColleagueInvitationByLeaderToMemberViewModel.ColleagueName),
                    nameof(ColleagueInvitationByLeaderToMemberViewModel.LeaderName),
                    nameof(ColleagueInvitationByLeaderToMemberViewModel.NonceAccept),
                    nameof(ColleagueInvitationByLeaderToMemberViewModel.NonceReject),
                    nameof(ColleagueInvitationByLeaderToMemberViewModel.Occupation)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.ColleagueAction_ByLeader_ColleagueInvitation_ToNonMember,
                Name = "Szakellátóhely vezető meghív egy munkatársat - munkatársnak, aki nem tag",
                Tokens = new HashSet<string>()
                {
                    nameof(ColleagueInvitationByLeaderToNonMemberViewModel.LeaderName),
                    nameof(ColleagueInvitationByLeaderToNonMemberViewModel.CenterName),
                    nameof(ColleagueInvitationByLeaderToNonMemberViewModel.ColleagueName),
                    nameof(ColleagueInvitationByLeaderToNonMemberViewModel.Occupation),
                    nameof(ColleagueInvitationByLeaderToNonMemberViewModel.AfterSignUpUrl)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.ColleagueAction_ByLeader_RejectApplication,
                Name = "Szakellátóhely vezető elutasítja egy munkatárs jelentkezését - munkatársnak",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.LeaderName),
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.CenterName),
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.ColleagueName),
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.Occupation),
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.Nonce)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.ColleagueAction_ByLeader_RemoveActive,
                Name = "Szakellátóhely vezető törli a már meglévő munkatársat - munkatársnak",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.LeaderName),
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.CenterName),
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.ColleagueName),
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.Occupation),
                    nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.Nonce)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.ColleagueAction_InvitationAccepted,
                Name = "Munkatárs elfogadja a meghívót - szakellátóhely vezetőnek",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileColleagueActionNotificationViewModel.LeaderName),
                    nameof(CenterProfileColleagueActionNotificationViewModel.CenterName),
                    nameof(CenterProfileColleagueActionNotificationViewModel.ColleagueName),
                    nameof(CenterProfileColleagueActionNotificationViewModel.Occupation),
                    nameof(CenterProfileColleagueActionNotificationViewModel.Nonce)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.ColleagueAction_InvitationRejected,
                Name = "Munkatárs elutasítja a meghívót - szakellátóhely vezetőnek",
                Tokens = new HashSet<string>()
                {
                    nameof(CenterProfileColleagueActionNotificationViewModel.LeaderName),
                    nameof(CenterProfileColleagueActionNotificationViewModel.CenterName),
                    nameof(CenterProfileColleagueActionNotificationViewModel.ColleagueName),
                    nameof(CenterProfileColleagueActionNotificationViewModel.Occupation),
                    nameof(CenterProfileColleagueActionNotificationViewModel.Nonce)
                }
            },
            new EmailTemplate()
            {
                Id = EmailTemplateIds.RenewalPeriodSubmissionReminder,
                Name = "Megújítási időszak - emlékeztető üzenet a szakellátóhely vezetőknek",
                Tokens = new HashSet<string>()
                {
                    nameof(RenewalPeriodSubmissionReminderViewModel.CenterName),
                    nameof(RenewalPeriodSubmissionReminderViewModel.LeaderName),
                    nameof(RenewalPeriodSubmissionReminderViewModel.Deadline),
                    nameof(RenewalPeriodSubmissionReminderViewModel.Nonce)
                }
            }
        };


        public EmailTemplateProvider(IEmailDataProcessorFactory emailDataProcessorFactory)
            : base(emailDataProcessorFactory)
        {
        }


        public override Task<IEnumerable<EmailTemplate>> GetEmailTemplatesAsync()
            => Task.FromResult(_emailTemplates);
    }
}
