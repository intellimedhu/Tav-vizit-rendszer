using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrganiMedCore Core",
    Author = "IntelliMed",
    Website = "http://intellimed.eu",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = "OrganiMedCore.Core",
    Name = "OrganiMedCore Core",
    Description = "Core features for the OrganiMedCore project.",
    Category = "IntelliMed",
    Dependencies = new string[]
    {
        "OrchardCore.Admin",
        "OrchardCore.Lucene"
    }
)]