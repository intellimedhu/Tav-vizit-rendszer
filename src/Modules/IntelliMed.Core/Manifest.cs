using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "IntelliMed Core",
    Author = "IntelliMed",
    Website = "http://intellimed.eu",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = "IntelliMed.Core",
    Name = "IntelliMed Core",
    Description = "Core features for IntelliMed.",
    Category = "IntelliMed",
    Dependencies = new[]
    {
        "OrchardCore.ContentManagement",
        "OrchardCore.Settings",
        "OrchardCore.Users"
    }
)]