using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrganiMedCore Diabetes Care Center Manager",
    Author = "IntelliMed",
    Website = "http://intellimed.eu",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = "OrganiMedCore.DiabetesCareCenterManager",
    Name = "OrganiMedCore Diabetes Care Center Manager",
    Description = "Core features for the Diabetes Care Center Manager tenant.",
    Category = "IntelliMed",
    Dependencies = new[]
    {
        "IntelliMed.Core",
        "OrganiMedCore.Core",
        "OrganiMedCore.Login",
        "OrganiMedCore.DiabetesCareCenter.Core",
        "OrganiMedCore.Email",
        "OrganiMedCore.DiabetesCareCenterManager.CenterProfileEditor",
        "OrganiMedCore.UriAuthentication",

        "OrchardCore.Admin",
        "OrchardCore.BackgroundTasks",
        "OrchardCore.ContentFields",
        "OrchardCore.ContentManagement",
        "OrchardCore.ContentManagement.Display",
        "OrchardCore.CustomSettings",
        "OrchardCore.Email",
        "OrchardCore.Users"
    }
)]