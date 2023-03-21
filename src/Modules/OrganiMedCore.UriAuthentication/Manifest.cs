using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrganiMedCore UriAuthentication",
    Author = "IntelliMed",
    Website = "http://intellimed.eu",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = "OrganiMedCore.UriAuthentication",
    Name = "OrganiMedCore UriAuthentication",
    Description = "Uri authentication features",
    Category = "IntelliMed",
    Dependencies = new[]
    {
        "OrchardCore.BackgroundTasks",
        "OrchardCore.ContentManagement",
        "OrchardCore.Settings",
        "OrchardCore.Users",

        "IntelliMed.Core",
        "IntelliMed.DokiNetIntegration",
        "OrganiMedCore.Login"
    }
)]