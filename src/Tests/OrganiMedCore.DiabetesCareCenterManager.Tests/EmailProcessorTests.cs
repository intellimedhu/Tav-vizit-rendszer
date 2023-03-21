using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Extension;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.DiabetesCareCenterManager.Email;
using OrganiMedCore.Email.Services;
using OrganiMedCore.Testing.Core.MockServices;
using OrganiMedCore.UriAuthentication.Models;
using OrganiMedCore.UriAuthentication.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class EmailProcessorTests
    {
        [Fact]
        public async Task Process_ShouldThrow()
        {
            await CheckPrerequisites(new CenterProfileAcceptedEmailProcessor(null));
            await CheckPrerequisites(new CenterProfileAssignedOnTenantEmailProcessor(null));
            await CheckPrerequisites(new CenterProfileCreatedEmailProcessor());
            await CheckPrerequisites(new CenterProfileCreatedFeedbackEmailProcessor());
            await CheckPrerequisites(new CenterProfileLeaderChangedEmailProcessor(null));
            await CheckPrerequisites(new CenterProfileLeaderChangedBeforeAssignmentEmailProcessor());
            await CheckPrerequisites(new CenterProfileRejectedEmailProcessor(null, null));
            await CheckPrerequisites(new CenterProfileSubmissionEmailProcessor(null));
            await CheckPrerequisites(new CenterProfileSubmissionFeedbackEmailProcessor());
            await CheckPrerequisites(new ColleagueActionApplicationByLeaderAcceptApplicationEmailProcessor(null, null));
            await CheckPrerequisites(new ColleagueActionApplicationByLeaderCancelInvitationToMemberEmailProcessor(null, null));
            await CheckPrerequisites(new ColleagueActionApplicationByLeaderCancelInvitationToNonMemberEmailProcessor());
            await CheckPrerequisites(new ColleagueActionApplicationByLeaderColleagueInvitationToMemberEmailProcessor(null, null));
            await CheckPrerequisites(new ColleagueActionApplicationByLeaderColleagueInvitationToNonMemberEmailProcessor(null));
            await CheckPrerequisites(new ColleagueActionApplicationByLeaderRejectApplicationEmailProcessor(null, null));
            await CheckPrerequisites(new ColleagueActionApplicationByLeaderRemoveActiveEmailProcessor(null, null));
            await CheckPrerequisites(new ColleagueActionApplicationCancelledEmailProcessor(null, null));
            await CheckPrerequisites(new ColleagueActionApplicationSubmittedEmailProcessor(null, null));
            await CheckPrerequisites(new ColleagueActionInvitationAcceptedEmailProcessor(null, null));
            await CheckPrerequisites(new ColleagueActionInvitationRejectedEmailProcessor(null, null));
            await CheckPrerequisites(new RenewalPeriodSubmissionReminderEmailProcessor(null));
        }

        [Theory]
        [InlineData(CenterPosts.TerritorialRapporteur, "területi referens")]
        [InlineData(CenterPosts.MDTSecretary, "területi referens")]
        [InlineData(CenterPosts.OMKB, "MDT-OMKB")]
        [InlineData(CenterPosts.MDTManagement, "MDT vezetőség")]
        public async Task CenterProfileAccepted_Process_ShouldProcess(string currentRole, string currentRoleCaption)
        {
            var data = new CenterProfileAcceptedViewModel()
            {
                AccreditationStatus = AccreditationStatus.Accredited,
                CenterName = "Sample center",
                CurrentRole = currentRole,
                PersonName = "Sample doctor"
            };
            var rawBody =
                $"-{nameof(CenterProfileAcceptedViewModel.AccreditationStatus)}-;" +
                $"-{nameof(CenterProfileAcceptedViewModel.CenterName)}-;" +
                $"-{nameof(CenterProfileAcceptedViewModel.CurrentRole)}-;" +
                $"-{nameof(CenterProfileAcceptedViewModel.PersonName)}-";

            var localizer = new LocalizerMock<CenterProfileAcceptedEmailProcessor>();
            var accr = AccreditationStatusCaptions.GetLocalizedValues(localizer)
                .First(x => x.Key == AccreditationStatus.Accredited)
                .Value;

            var processor = new CenterProfileAcceptedEmailProcessor(localizer);

            var result = await processor.ProcessAsync(data, rawBody);

            Assert.Equal($"{accr};{data.CenterName};{currentRoleCaption};{data.PersonName}", result);
        }

        [Fact]
        public async Task CenterProfileAssignedOnTenant_Process_ShouldProcess()
        {
            var data = new CenterProfileAssignedOnTenantViewModel()
            {
                CenterName = "CN",
                LeaderName = "LN"
            };

            var rawBody =
                $"-{nameof(CenterProfileAssignedOnTenantViewModel.CenterName)}-;" +
                $"-{nameof(CenterProfileAssignedOnTenantViewModel.LeaderName)}-;" +
                $"-{nameof(CenterProfileAssignedOnTenantViewModel.Nonce)}-";

            var uri = "http://www.fake.uri/";

            var processor = new CenterProfileAssignedOnTenantEmailProcessor(
                new NonceServiceMock2(uri));

            var result = await processor.ProcessAsync(data, rawBody);

            Assert.Equal($"{data.CenterName};{data.LeaderName};{uri}", result);
        }

        [Fact]
        public async Task CenterProfileCreatedEmailProcessor_Process_ShouldProcess()
        {
            var data = new CenterProfileCreatedTemplateViewModel()
            {
                CenterName = "CN",
                LeaderName = "LN"
            };

            var rawBody =
                $"-{nameof(CenterProfileCreatedTemplateViewModel.CenterName)}-;" +
                $"-{nameof(CenterProfileCreatedTemplateViewModel.LeaderName)}-";

            var processor = new CenterProfileCreatedEmailProcessor();

            var result = await processor.ProcessAsync(data, rawBody);

            Assert.Equal($"{data.CenterName};{data.LeaderName}", result);
        }

        [Fact]
        public async Task CenterProfileCreatedFeedbackEmailProcessor_Process_ShouldProcess()
        {
            var data = new CenterProfileCreatedTemplateViewModel()
            {
                CenterName = "CN",
                LeaderName = "LN"
            };

            var rawBody =
                $"-{nameof(CenterProfileCreatedTemplateViewModel.CenterName)}-;" +
                $"-{nameof(CenterProfileCreatedTemplateViewModel.LeaderName)}-";

            var processor = new CenterProfileCreatedFeedbackEmailProcessor();

            var result = await processor.ProcessAsync(data, rawBody);

            Assert.Equal($"{data.CenterName};{data.LeaderName}", result);
        }

        [Fact]
        public async Task CenterProfileLeaderChanged_Process_ShouldProcess()
        {
            var data = new CenterProfileLeaderChanged()
            {
                CenterName = "cn",
                NewLeaderName = "nln"
            };

            var rawBody =
                $"-{nameof(CenterProfileLeaderChanged.CenterName)}-;" +
                $"-{nameof(CenterProfileLeaderChanged.NewLeaderName)}-;" +
                $"-{nameof(CenterProfileLeaderChanged.NewLeaderName)}-;" +
                $"-{nameof(CenterProfileLeaderChanged.NewLeaderName)}-;" +
                $"-{nameof(CenterProfileLeaderChanged.Nonce)}-";

            var uri = "fakeuri";

            var processor = new CenterProfileLeaderChangedEmailProcessor(
                new NonceServiceMock2(uri));

            var result = await processor.ProcessAsync(data, rawBody);

            Assert.Equal($"{data.CenterName};{data.NewLeaderName};{data.NewLeaderName};{data.NewLeaderName};{uri}", result);
        }

        [Fact]
        public async Task CenterProfileLeaderChangedBeforeAssignment_Process_ShouldProcess()
        {
            var data = new CenterProfileLeaderChanged()
            {
                CenterName = "cn",
                NewLeaderName = "nln"
            };

            var rawBody =
                $"-{nameof(CenterProfileLeaderChanged.CenterName)}-;" +
                $"-{nameof(CenterProfileLeaderChanged.NewLeaderName)}-";

            var processor = new CenterProfileLeaderChangedBeforeAssignmentEmailProcessor();

            var result = await processor.ProcessAsync(data, rawBody);

            Assert.Equal($"{data.CenterName};{data.NewLeaderName}", result);
        }

        [Theory]
        [InlineData(CenterPosts.TerritorialRapporteur, "területi referens")]
        [InlineData(CenterPosts.MDTSecretary, "területi referens")]
        [InlineData(CenterPosts.OMKB, "MDT-OMKB")]
        [InlineData(CenterPosts.MDTManagement, "MDT vezetőség")]
        public async Task CenterProfileRejectedEmailProcessor_Process_ShouldProcess(string currentRole, string currentRoleCaption)
        {
            var data = new CenterProfileRejectedViewModel()
            {
                CenterName = "cn",
                CurrentRole = currentRole,
                PersonName = "pn",
                RejectReason = "rr"
            };
            var rawBody =
                $"-{nameof(CenterProfileRejectedViewModel.CenterName)}-;" +
                $"-{nameof(CenterProfileRejectedViewModel.CurrentRole)}-;" +
                $"-{nameof(CenterProfileRejectedViewModel.PersonName)}-;" +
                $"-{nameof(CenterProfileRejectedViewModel.Nonce)}-;" +
                $"-{nameof(CenterProfileRejectedViewModel.RejectReason)}-";

            var url = "fakeurl";
            var processor = new CenterProfileRejectedEmailProcessor(
                new LocalizerMock<CenterProfileRejectedEmailProcessor>(),
                new NonceServiceMock2(url));

            var result = await processor.ProcessAsync(data, rawBody);

            Assert.Equal($"{data.CenterName};{currentRoleCaption};{data.PersonName};{url};{data.RejectReason}", result);
        }

        [Fact]
        public async Task CenterProfileSubmission_Process_ShouldProcess()
        {
            var data = new CenterProfileSubmissionViewModel()
            {
                CenterLeaderName = "cln",
                CenterName = "cn",
                ReviewerMemberFullName = "rmfn"
            };
            var uri = "fake-uri";
            var processor = new CenterProfileSubmissionEmailProcessor(
                new NonceServiceMock2(uri));

            var result = await processor.ProcessAsync(
                data,
                $"-{nameof(CenterProfileSubmissionViewModel.CenterName)}-.-{nameof(CenterProfileSubmissionViewModel.CenterLeaderName)}-!" +
                $"-{nameof(CenterProfileSubmissionViewModel.Nonce)}--{nameof(CenterProfileSubmissionViewModel.ReviewerMemberFullName)}-");

            Assert.Equal(result, $"{data.CenterName}.{data.CenterLeaderName}!{uri}{data.ReviewerMemberFullName}");
        }

        [Fact]
        public async Task CenterProfileSubmissionFeecback_Process_ShouldProcess()
        {
            var data = new CenterProfileSubmissionFeedbackViewModel()
            {
                CenterLeaderName = "cln",
                CenterName = "cn",
                Reviewers = new[] { "C", "B", "X", "A" }
            };
            var processor = new CenterProfileSubmissionFeedbackEmailProcessor();

            var result = await processor.ProcessAsync(
                data,
                $"-{nameof(CenterProfileSubmissionFeedbackViewModel.CenterName)}-.-{nameof(CenterProfileSubmissionFeedbackViewModel.CenterLeaderName)}-!" +
                $"-{nameof(CenterProfileSubmissionFeedbackViewModel.Reviewers)}-");

            Assert.Equal(result, $"{data.CenterName}.{data.CenterLeaderName}!A, B, C, X");
        }

        [Theory]
        [InlineData(Occupation.CommunityNurse)]
        [InlineData(Occupation.DiabetesNurseEducator)]
        [InlineData(Occupation.Dietician)]
        [InlineData(Occupation.Doctor)]
        [InlineData(Occupation.Nurse)]
        [InlineData(Occupation.Other)]
        [InlineData(Occupation.Physiotherapist)]
        public async Task ColleagueActionApplicationByLeaderEmailProcessors_Process_ShouldProcess(Occupation occupation)
        {
            var data = new CenterProfileColleagueAsMemberNotificationByLeaderViewModel()
            {
                CenterName = "cn1",
                ColleagueName = "cn2",
                LeaderName = "ln",
                Occupation = occupation
            };

            var uri = "https://mydomain.com";
            var nonceServiceMock = new NonceServiceMock2(uri);

            // All items that are derivable from ColleagueActionApplicationByLeaderEmailProcessorBase
            var processors = new IEmailDataProcessor[]
            {
                new ColleagueActionApplicationByLeaderAcceptApplicationEmailProcessor(
                    new LocalizerMock<ColleagueActionApplicationByLeaderAcceptApplicationEmailProcessor>(), nonceServiceMock),
                new ColleagueActionApplicationByLeaderCancelInvitationToMemberEmailProcessor(
                    new LocalizerMock<ColleagueActionApplicationByLeaderCancelInvitationToMemberEmailProcessor>(), nonceServiceMock),
                new ColleagueActionApplicationByLeaderRejectApplicationEmailProcessor(
                    new LocalizerMock<ColleagueActionApplicationByLeaderRejectApplicationEmailProcessor>(), nonceServiceMock),
                new ColleagueActionApplicationByLeaderRemoveActiveEmailProcessor(
                    new LocalizerMock<ColleagueActionApplicationByLeaderRemoveActiveEmailProcessor>(), nonceServiceMock)
            };

            var rawBody =
                $"-{nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.CenterName)}-" +
                $"-{nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.ColleagueName)}-" +
                $"-{nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.LeaderName)}-" +
                $"-{nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.Nonce)}-" +
                $"-{nameof(CenterProfileColleagueAsMemberNotificationByLeaderViewModel.Occupation)}-";

            var results = await Task.WhenAll(processors.Select(p => p.ProcessAsync(data, rawBody)));

            var occupationText = OccupationExtensions.GetLocalizedValues(new LocalizerMock<IEmailDataProcessor>())
                .First(x => x.Key == occupation)
                .Value;

            Assert.All(
                results,
                r => r.Equals(
                    $"{data.CenterName}{data.ColleagueName}{data.LeaderName}{uri}{occupationText}"));
        }

        [Fact]
        public async Task ColleagueActionApplicationByLeaderCancelInvitationToNonMember_Process_ShouldProcess()
        {
            var data = new CenterProfileColleagueActionNotificationViewModel()
            {
                CenterName = "cn",
                LeaderName = "ln"
            };

            var rawBody =
                $"-{nameof(CenterProfileColleagueActionNotificationViewModel.CenterName)}-\n" +
                $"-{nameof(CenterProfileColleagueActionNotificationViewModel.LeaderName)}-\n" +
                $"-{nameof(CenterProfileColleagueActionNotificationViewModel.ColleagueName)}-";

            var processor = new ColleagueActionApplicationByLeaderCancelInvitationToNonMemberEmailProcessor();

            var result = await processor.ProcessAsync(data, rawBody);

            Assert.Equal($"{data.CenterName}\n{data.LeaderName}\n{data.ColleagueName}", result);
        }

        [Theory]
        [InlineData(Occupation.CommunityNurse)]
        [InlineData(Occupation.DiabetesNurseEducator)]
        [InlineData(Occupation.Dietician)]
        [InlineData(Occupation.Doctor)]
        [InlineData(Occupation.Nurse)]
        [InlineData(Occupation.Other)]
        [InlineData(Occupation.Physiotherapist)]
        public async Task ColleagueActionApplicationByLeaderColleagueInvitationToMember_Process_ShouldProcess(Occupation occupation)
        {
            var data = new ColleagueInvitationByLeaderToMemberViewModel()
            {
                CenterName = "c",
                LeaderName = "lll",
                ColleagueName = "ff",
                Occupation = occupation
            };

            var rawBody =
                $"-{nameof(ColleagueInvitationByLeaderToMemberViewModel.CenterName)}-\n" +
                $"-{nameof(ColleagueInvitationByLeaderToMemberViewModel.Occupation)}-\n" +
                $"-{nameof(ColleagueInvitationByLeaderToMemberViewModel.LeaderName)}-\n" +
                $"-{nameof(ColleagueInvitationByLeaderToMemberViewModel.NonceReject)}-\n" +
                $"-{nameof(ColleagueInvitationByLeaderToMemberViewModel.NonceAccept)}-\n" +
                $"-{nameof(ColleagueInvitationByLeaderToMemberViewModel.ColleagueName)}-";

            var occupationText = OccupationExtensions.GetLocalizedValues(new LocalizerMock<IEmailDataProcessor>())
                .First(x => x.Key == occupation)
                .Value;

            var processor = new ColleagueActionApplicationByLeaderColleagueInvitationToMemberEmailProcessor(
                new LocalizerMock<ColleagueActionApplicationByLeaderColleagueInvitationToMemberEmailProcessor>(),
                new NonceServiceMock2(nameof(data)));

            var result = await processor.ProcessAsync(data, rawBody);

            Assert.Equal($"{data.CenterName}\n{occupationText}\n{data.LeaderName}\n{nameof(data)}\n{nameof(data)}\n{data.ColleagueName}", result);
        }

        [Theory]
        [InlineData(Occupation.CommunityNurse)]
        [InlineData(Occupation.DiabetesNurseEducator)]
        [InlineData(Occupation.Dietician)]
        [InlineData(Occupation.Doctor)]
        [InlineData(Occupation.Nurse)]
        [InlineData(Occupation.Other)]
        [InlineData(Occupation.Physiotherapist)]
        public async Task ColleagueActionApplicationByLeaderColleagueInvitationToNonMember_Process_ShouldProcess(Occupation occupation)
        {
            var data = new ColleagueInvitationByLeaderToNonMemberViewModel()
            {
                CenterName = "c",
                LeaderName = "lll",
                ColleagueName = "ff",
                Occupation = occupation,
                AfterSignUpUrl = "http://127.0.0.1/"
            };

            var rawBody =
                $"-{nameof(ColleagueInvitationByLeaderToNonMemberViewModel.CenterName)}-\n" +
                $"-{nameof(ColleagueInvitationByLeaderToNonMemberViewModel.Occupation)}-\n" +
                $"-{nameof(ColleagueInvitationByLeaderToNonMemberViewModel.LeaderName)}-\n" +
                $"-{nameof(ColleagueInvitationByLeaderToNonMemberViewModel.AfterSignUpUrl)}-\n" +
                $"-{nameof(ColleagueInvitationByLeaderToNonMemberViewModel.ColleagueName)}-";

            var occupationText = OccupationExtensions.GetLocalizedValues(new LocalizerMock<IEmailDataProcessor>())
                .First(x => x.Key == occupation)
                .Value;

            var processor = new ColleagueActionApplicationByLeaderColleagueInvitationToNonMemberEmailProcessor(
                new LocalizerMock<ColleagueActionApplicationByLeaderColleagueInvitationToNonMemberEmailProcessor>());

            var result = await processor.ProcessAsync(data, rawBody);

            Assert.Equal($"{data.CenterName}\n{occupationText}\n{data.LeaderName}\n{data.AfterSignUpUrl}\n{data.ColleagueName}", result);
        }

        [Fact]
        public async Task RenewalPeriodSubmissionReminder_Process_ShouldProcess()
        {
            var data = new RenewalPeriodSubmissionReminderViewModel()
            {
                CenterName = "1",
                Deadline = DateTime.Parse("2000-02-22 22:22"),
                LeaderName = "3"
            };

            var processor = new RenewalPeriodSubmissionReminderEmailProcessor(new NonceServiceMock2(nameof(data)));
            var result = await processor.ProcessAsync(
                data,
                $"-{nameof(RenewalPeriodSubmissionReminderViewModel.Nonce)}-|" +
                $"-{nameof(RenewalPeriodSubmissionReminderViewModel.LeaderName)}-|" +
                $"-{nameof(RenewalPeriodSubmissionReminderViewModel.CenterName)}-|" +
                $"-{nameof(RenewalPeriodSubmissionReminderViewModel.Deadline)}-|");

            Assert.Equal(
                $"{nameof(data)}|{data.LeaderName}|{data.CenterName}|{data.Deadline.ToLocalTime().ToString("yyyy.MM.dd. HH:mm")}|",
                result);
        }


        private async Task CheckPrerequisites<T>(T processor)
            where T : IEmailDataProcessor
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => processor.ProcessAsync(null, "rb"));
            await Assert.ThrowsAsync<ArgumentNullException>(() => processor.ProcessAsync(new { }, null));
            await Assert.ThrowsAsync<ArgumentException>(() => processor.ProcessAsync(new { }, "rb"));
        }
    }

    class NonceServiceMock2 : INonceService
    {
        private readonly string _expectedUri;


        public NonceServiceMock2(string expectedUri)
        {
            _expectedUri = expectedUri;
        }


        public Task<string> GetUriAsync(Guid value) => Task.FromResult(_expectedUri);

        [ExcludeFromCodeCoverage]
        public Task CleanupAsync()
            => throw new NotImplementedException();

        [ExcludeFromCodeCoverage]
        public Task CreateAsync(Nonce nonce)
            => throw new NotImplementedException();

        [ExcludeFromCodeCoverage]
        public Task CreateManyAsync(IEnumerable<Nonce> nonces)
            => throw new NotImplementedException();

        [ExcludeFromCodeCoverage]
        public Task<Nonce> GetByValue(Guid value)
            => throw new NotImplementedException();
    }
}
