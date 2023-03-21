using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Models;
using Newtonsoft.Json;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Views;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class CenterProfileComplexViewModel : ShapeViewModel
    {
        [JsonProperty("contentItemId")]
        public string ContentItemId { get; set; }

        [JsonProperty("leader")]
        public CenterProfileLeaderViewModel Leader { get; set; }

        [JsonProperty("basicData")]
        public CenterProfileBasicDataViewModel BasicData { get; set; }

        [JsonProperty("additional")]
        public CenterProfileAdditionalDataViewModel Additional { get; set; }

        [JsonProperty("equipments")]
        public CenterProfileEquipmentsViewModel Equipments { get; set; }

        [JsonProperty("colleagues")]
        public CenterProfileColleaguesViewModel Colleagues { get; set; }

        [JsonProperty("renewal")]
        public CenterProfileRenewalViewModel Renewal { get; set; }


        public void UpdateBasicData(ContentItem contentItem)
        {
            ThrowIfNullOrBadType(contentItem);

            if (BasicData == null)
            {
                BasicData = new CenterProfileBasicDataViewModel();
            }

            BasicData.UpdateViewModel(contentItem.As<CenterProfilePart>());
        }

        public void UpdateAdditional(ContentItem contentItem)
        {
            ThrowIfNullOrBadType(contentItem);

            if (Additional == null)
            {
                Additional = new CenterProfileAdditionalDataViewModel();
            }

            Additional.UpdateViewModel(contentItem.As<CenterProfilePart>());
        }

        public void UpdateRenewal(ContentItem contentItem)
        {
            ThrowIfNullOrBadType(contentItem);

            if (Renewal == null)
            {
                Renewal = new CenterProfileRenewalViewModel();
            }

            Renewal.UpdateViewModel(contentItem.As<CenterProfileManagerExtensionsPart>());
        }

        public void UpdateLeader(DokiNetMember member)
        {
            member.ThrowIfNull();
            if (Leader == null)
            {
                Leader = new CenterProfileLeaderViewModel();
            }

            Leader.FullName = member.FullName;
            Leader.MemberRightId = member.MemberRightId;
            Leader.Email = member.Emails.FirstOrDefault();
        }

        public void UpdateEquipments(ContentItem contentItem)
        {
            ThrowIfNullOrBadType(contentItem);

            if (Equipments == null)
            {
                Equipments = new CenterProfileEquipmentsViewModel();
            }

            Equipments.UpdateViewModel(contentItem.As<CenterProfilePart>());
        }

        public void UpdateColleagues(ContentItem contentItem)
        {
            ThrowIfNullOrBadType(contentItem);

            if (Colleagues == null)
            {
                Colleagues = new CenterProfileColleaguesViewModel();
            }

            Colleagues.UpdateViewModel(contentItem.As<CenterProfilePart>());
        }


        public static CenterProfileComplexViewModel CreateViewModel(
            ContentItem contentItem,
            bool basicData = false,
            bool additional = false,
            bool renewal = false,
            bool equipments = false,
            bool colleagues = false,
            DokiNetMember member = null)
        {
            ThrowIfNullOrBadType(contentItem);

            var viewModel = new CenterProfileComplexViewModel()
            {
                ContentItemId = contentItem.ContentItemId
            };

            if (basicData)
            {
                viewModel.UpdateBasicData(contentItem);
            }

            if (additional)
            {
                viewModel.UpdateAdditional(contentItem);
            }

            if (renewal)
            {
                viewModel.UpdateRenewal(contentItem);
            }

            if (equipments)
            {
                viewModel.UpdateEquipments(contentItem);
            }

            if (colleagues)
            {
                viewModel.UpdateColleagues(contentItem);
            }

            if (member != null)
            {
                viewModel.UpdateLeader(member);
            }

            return viewModel;
        }


        private static void ThrowIfNullOrBadType(ContentItem contentItem)
        {
            contentItem.ThrowIfNull();
            if (contentItem.ContentType != ContentTypes.CenterProfile)
            {
                throw new ArgumentException();
            }
        }
    }
}
