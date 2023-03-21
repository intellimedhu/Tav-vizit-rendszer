using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrganiMedCore.DiabetesCareCenter.Core;
using OrganiMedCore.DiabetesCareCenterManager.Constants;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager
{
    public class AdminMenu : INavigationProvider
    {
        public IStringLocalizer T { get; set; }


        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }


        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(T["Szakellátóhelyek"], "50", config => config
                    .Id("dccmnavlink")
                    .Permission(OrchardCore.Admin.Permissions.AccessAdminPanel)
                    .Add(T["Települések"], "1", settlements => settlements
                        .Add(T["Importálás"], zipCodes => zipCodes
                            .Action("Import", "Settlements", new { area = "OrganiMedCore.DiabetesCareCenterManager" })
                            .Permission(OrchardCore.Admin.Permissions.AccessAdminPanel)
                            .LocalNav()
                        )
                        .Add(T["Lista"], zipCodes => zipCodes
                            .Action("Index", "Settlements", new { area = "OrganiMedCore.DiabetesCareCenterManager" })
                            .Permission(OrchardCore.Admin.Permissions.AccessAdminPanel)
                            .LocalNav()
                        )
                    )
                    .Add(T["Import"], "2", import => import
                        .Action("Index", "DataMigration", new { area = "OrganiMedCore.DiabetesCareCenterManager" })
                        .Permission(OrchardCore.Admin.Permissions.AccessAdminPanel)
                        .LocalNav()
                    )
                    .Add(T["Akkreditációs feltételek"], "3", conditionis => conditionis
                        .Add(T["Személyi"], "10", personalConditions => personalConditions
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GroupIds.PersonalConditionSettings })
                            .Permission(ManagerPermissions.ManageCenterProfileAccreditationConditionsSettings)
                            .LocalNav()
                        )
                        .Add(T["Tárgyi"], equipments => equipments
                            .Action("Index", "CenterProfileEquipments", new { area = "OrganiMedCore.DiabetesCareCenterManager" })
                            .Permission(ManagerPermissions.ManageCenterProfileAccreditationConditionsSettings)
                            .LocalNav()
                        )
                    )
                    .Add(T["Megújítási időszak"], "4", renewal => renewal
                        .Action("Index", "CenterSettingsRenewalPeriod", new { area = "OrganiMedCore.DiabetesCareCenterManager" })
                        .Permission(OrchardCore.Admin.Permissions.AccessAdminPanel)
                        .LocalNav()
                    )
                    .Add(T["Szakképesítések"], "5", qualifications => qualifications
                        .Action("Index", "CenterProfileQualifications", new { area = "OrganiMedCore.DiabetesCareCenterManager" })
                        .Permission(OrchardCore.Admin.Permissions.AccessAdminPanel)
                        .LocalNav()
                    )
                    .Add(T["Beállítások"], "100", crSettings => crSettings
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GroupIds.CenterProfileSettings })
                        .Permission(OrchardCore.Admin.Permissions.AccessAdminPanel)
                        .LocalNav()
                    )
                    .Add(T["Adatlapok, tenantok"], "110", crSettings => crSettings
                        .Action("List", "CenterProfileAdmin", new { area = "OrganiMedCore.DiabetesCareCenterManager" })
                        .Permission(ManagerPermissions.CreateCenterProfileTenant)
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
