using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrganiMedCore Organization DiabetesCare",
    Author = "IntelliMed",
    Website = "http://intellimed.eu",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = "OrganiMedCore.Organization.DiabetesCare",
    Name = "OrganiMedCore Organization DiabetesCare",
    Description = "AGP+ feature for the organization tenants.",
    Category = "IntelliMed",
    Dependencies = new[]
    {
        "IntelliMed.Core",
        "OrganiMedCore.Core",
        "OrganiMedCore.Organization"
    }
)]