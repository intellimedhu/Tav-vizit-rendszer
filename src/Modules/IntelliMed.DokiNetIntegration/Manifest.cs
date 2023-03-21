using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "IntelliMed - doki.NET Integration",
    Author = "IntelliMed",
    Website = "http://intellimed.eu",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = "IntelliMed.DokiNetIntegration",
    Name = "IntelliMed - doki.NET Integration",
    Description = "Provides features to communication with doki.NET",
    Category = "IntelliMed",
    Dependencies = new string[]
    {
        "OrchardCore.Admin"
    }
)]