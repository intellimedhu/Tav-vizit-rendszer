using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrganiMedCore Organization",
    Author = "IntelliMed",
    Website = "http://intellimed.eu",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = "OrganiMedCore.Organization",
    Name = "OrganiMedCore Organization",
    Description = "Core features for the organization tenants.",
    Category = "IntelliMed",
    Dependencies = new[]
    {
        "IntelliMed.Core",
        "OrganiMedCore.Core",
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