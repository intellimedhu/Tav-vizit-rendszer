using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrganiMedCore Manager",
    Author = "IntelliMed",
    Website = "http://intellimed.eu",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = "OrganiMedCore.Manager",
    Name = "OrganiMedCore Manager",
    Description = "Core features for the manager tenant.",
    Category = "IntelliMed",
    Dependencies = new[]
    {
        "IntelliMed.Core",
        "IntelliMed.DokiNetIntegration",
        "OrganiMedCore.Core",

        "OrchardCore.Admin",
        "OrchardCore.ContentFields",
        "OrchardCore.ContentManagement",
        "OrchardCore.ContentManagement.Display",
        "OrchardCore.CustomSettings",
        "OrchardCore.Email",
        "OrchardCore.Users"
    }
)]