using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrganiMedCore Diabetes Care Center Core",
    Author = "IntelliMed",
    Website = "http://intellimed.eu",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = "OrganiMedCore.DiabetesCareCenter.Core",
    Name = "OrganiMedCore Diabetes Care Center Core",
    Description = "Core features for both of Diabetes Care Center Manager and Diabetes Care Center tenants.",
    Category = "IntelliMed",
    Dependencies = new[]
    {
        "IntelliMed.Core",
        "IntelliMed.DokiNetIntegration",

        "OrchardCore.Admin",
        "OrchardCore.ContentFields",
        "OrchardCore.ContentManagement",
        "OrchardCore.ContentManagement.Display",
        "OrchardCore.CustomSettings",
        "OrchardCore.Email",
        "OrchardCore.Users"
    }
)]