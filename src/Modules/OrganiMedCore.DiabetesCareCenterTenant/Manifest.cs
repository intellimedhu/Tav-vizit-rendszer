using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrganiMedCore Diabetes Care Center Tenant",
    Author = "IntelliMed",
    Website = "http://intellimed.eu",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = "OrganiMedCore.DiabetesCareCenterTenant",
    Name = "OrganiMedCore Diabetes Care Center Tenant",
    Description = "Diabetes care center tenant features.",
    Category = "IntelliMed",
    Dependencies = new[]
    {
        "IntelliMed.Core",
        "IntelliMed.DokiNetIntegration",
        "OrganiMedCore.DiabetesCareCenter.Core",
        "OrganiMedCore.DiabetesCareCenterManager.CenterProfileEditor",
        "OrganiMedCore.Login",

        "OrchardCore.Admin",
        "OrchardCore.ContentFields",
        "OrchardCore.ContentManagement",
        "OrchardCore.ContentManagement.Display",
        "OrchardCore.CustomSettings",
        "OrchardCore.Email",
        "OrchardCore.Lucene",
        "OrchardCore.Users"
    }
)]