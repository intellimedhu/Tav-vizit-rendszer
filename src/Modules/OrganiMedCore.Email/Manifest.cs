using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrganiMedCore Email",
    Author = "IntelliMed",
    Website = "http://intellimed.eu",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = "OrganiMedCore.Email",
    Name = "OrganiMedCore Email",
    Description = "Email-related features for the OrganiMedCore project.",
    Category = "IntelliMed",
    Dependencies = new string[]
    {
        "OrchardCore.Admin",
        "OrchardCore.BackgroundTasks",
        "OrchardCore.ContentFields",
        "OrchardCore.ContentManagement",
        "OrchardCore.ContentManagement.Display",
        "OrchardCore.CustomSettings",
        "OrchardCore.Email",
        "OrchardCore.Settings"
    }
)]