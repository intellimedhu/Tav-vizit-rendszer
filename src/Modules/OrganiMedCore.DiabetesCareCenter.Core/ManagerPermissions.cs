using IntelliMed.Core.Constants;
using OrchardCore.Security.Permissions;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core
{
    public class ManagerPermissions : IPermissionProvider
    {
        public static readonly Permission ManageCenterManagerUsers = new Permission(nameof(ManageCenterManagerUsers), "Manage Users in Diabetes Care Center Manager");
        public static readonly Permission ManageMDTManagement = new Permission(nameof(ManageMDTManagement), "Manage MDT Management");
        public static readonly Permission ManageMDTSecretary = new Permission(nameof(ManageMDTSecretary), "Manage MDT Secretary");
        public static readonly Permission ManageOMKB = new Permission(nameof(ManageOMKB), "Manage OMKB");
        public static readonly Permission ManageTerritorialRapporteurs = new Permission(nameof(ManageTerritorialRapporteurs), "Manage Territorial Rapporteurs");
        public static readonly Permission ViewTerritorialRapporteurs = new Permission(nameof(ViewTerritorialRapporteurs), "View list of Territorial Rapporteurs");
        public static readonly Permission ManageZipCodes = new Permission(nameof(ManageZipCodes), "Manage zip codes");
        public static readonly Permission ViewListOfCenterProfiles = new Permission(nameof(ViewListOfCenterProfiles), "View list of center profiles");
        public static readonly Permission ViewAllCenterProfiles = new Permission(nameof(ViewAllCenterProfiles), "View all center profiles");
        public static readonly Permission DeleteCenterProfile = new Permission(nameof(DeleteCenterProfile), "Delete center profile");
        public static readonly Permission ReviewCenterProfile = new Permission(nameof(ReviewCenterProfile), "Review center profile");
        public static readonly Permission MakingDecisionAboutCenterProfiles = new Permission(nameof(MakingDecisionAboutCenterProfiles), "Make decision about center profiles");
        public static readonly Permission ManageCenterProfileAccreditationConditionsSettings = new Permission(nameof(ManageCenterProfileAccreditationConditionsSettings), "Manage center profile accreditation conditions");
        public static readonly Permission ManageCenterProfileLeaders = new Permission(nameof(ManageCenterProfileLeaders), "Manage center profile leaders");
        public static readonly Permission CreateCenterProfile = new Permission(nameof(CreateCenterProfile), "Create center profile");
        public static readonly Permission CreateCenterProfileTenant = new Permission(nameof(CreateCenterProfileTenant), "Create tenant for center profile");


        public Task<IEnumerable<Permission>> GetPermissionsAsync()
            => Task.FromResult(new[]
            {
                ManageCenterManagerUsers,
                ManageMDTManagement,
                ManageMDTSecretary,
                ManageOMKB,
                ManageTerritorialRapporteurs,
                ViewTerritorialRapporteurs,
                ManageZipCodes,
                ViewListOfCenterProfiles,
                ViewAllCenterProfiles,
                DeleteCenterProfile,
                ReviewCenterProfile,
                MakingDecisionAboutCenterProfiles,
                ManageCenterProfileAccreditationConditionsSettings,
                ManageCenterProfileLeaders,
                CreateCenterProfile,
                CreateCenterProfileTenant
            }.AsEnumerable());

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
            => new[]
            {
                new PermissionStereotype()
                {
                    Name = WellKnownNames.AdminRoleName,
                    Permissions = new[]
                    {
                        ManageCenterManagerUsers,
                        ManageMDTManagement,
                        ManageMDTSecretary,
                        ManageOMKB,
                        ManageTerritorialRapporteurs,
                        ViewTerritorialRapporteurs,
                        ManageZipCodes,
                        ViewListOfCenterProfiles,
                        ViewAllCenterProfiles,
                        DeleteCenterProfile,
                        ReviewCenterProfile,
                        MakingDecisionAboutCenterProfiles,
                        ManageCenterProfileAccreditationConditionsSettings,
                        ManageCenterProfileLeaders,
                        CreateCenterProfile,
                        CreateCenterProfileTenant
                    }
                },
                new PermissionStereotype()
                {
                    Name = CenterPosts.MDTManagement,
                    Permissions = new []
                    {
                        ManageCenterManagerUsers,
                        ManageMDTSecretary,
                        ManageOMKB,
                        ManageTerritorialRapporteurs,
                        ManageZipCodes,
                        ViewListOfCenterProfiles,
                        ViewAllCenterProfiles,
                        DeleteCenterProfile,
                        ReviewCenterProfile,
                        MakingDecisionAboutCenterProfiles,
                        ManageCenterProfileAccreditationConditionsSettings,
                        ManageCenterProfileLeaders
                    }
                },
                new PermissionStereotype()
                {
                    Name = CenterPosts.MDTSecretary,
                    Permissions = new []
                    {
                        ManageCenterManagerUsers,
                        ManageTerritorialRapporteurs,
                        ManageZipCodes,
                        ViewListOfCenterProfiles,
                        ViewAllCenterProfiles,
                        DeleteCenterProfile,
                        ReviewCenterProfile,
                        ManageCenterProfileAccreditationConditionsSettings,
                        ManageCenterProfileLeaders
                    }
                },
                new PermissionStereotype()
                {
                    Name = CenterPosts.OMKB,
                    Permissions = new []
                    {
                        ManageCenterManagerUsers,
                        ViewTerritorialRapporteurs,                        
                        ManageZipCodes,
                        ViewListOfCenterProfiles,
                        ViewAllCenterProfiles,
                        ReviewCenterProfile
                    }
                },
                new PermissionStereotype()
                {
                    Name = CenterPosts.TerritorialRapporteur,
                    Permissions = new[]
                    {
                        ViewTerritorialRapporteurs,
                        ViewListOfCenterProfiles,
                        ReviewCenterProfile
                    }
                },
                new PermissionStereotype()
                {
                    Name = CenterPosts.Doctor,
                    Permissions = new[]
                    {
                        ViewTerritorialRapporteurs,
                        CreateCenterProfile
                    }
                }
            };
    }
}
