using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrganiMedCore Diabetes Care Center Widgets",
    Author = "IntelliMed",
    Website = "http://intellimed.eu",
    Version = "1.0.0",
    Dependencies = new[]
    {
        "OrganiMedCore.DiabetesCareCenter.Core"
    }
)]

[assembly: Feature(
    Id = "OrganiMedCore.DiabetesCareCenter.Widgets",
    Name = "OrganiMedCore Diabetes Care Center Widgets",
    Description = "Core features for manager and organization Widgets",
    Category = "IntelliMed"
)]

[assembly: Feature(
    Id = "OrganiMedCore.DiabetesCareCenterManager.Widgets",
    Name = "OrganiMedCore Diabetes Care Center Manager Widgets",
    Description = "Widgets for DCCM tenant",
    Category = "IntelliMed",
    Dependencies = new[]
    {
        "OrganiMedCore.DiabetesCareCenter.Widgets.Core"
    }
)]

[assembly: Feature(
    Id = "OrganiMedCore.Organization.DiabetesCareCenter.Widgets",
    Name = "OrganiMedCore Organization Diabetes Care Center Widgets",
    Description = "Widgets for DCC tenants",
    Category = "IntelliMed",
    Dependencies = new[]
    {
        "OrganiMedCore.DiabetesCareCenter.Widgets.Core"
    }
)]