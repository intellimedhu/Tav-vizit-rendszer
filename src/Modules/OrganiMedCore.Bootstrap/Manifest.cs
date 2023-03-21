using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrganiMedCore Bootstrap",
    Author = "IntelliMed",
    Website = "http://intellimed.eu",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = "OrganiMedCore.Bootstrap",
    Name = "OrganiMedCore Bootstrap",
    Description = "Contains boostrap related features.",
    Category = "IntelliMed",
    Dependencies = new[]
    {
        "OrchardCore.Module.Targets",
        "OrchardCore.Settings",
        "OrchardCore.ContentManagement",
        "OrchardCore.ContentFields",
        "OrchardCore.Contents",
        "OrchardCore.ContentTypes",
        "IntelliMed.Core"
    }
)]