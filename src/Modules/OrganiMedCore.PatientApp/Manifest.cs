using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrganiMedCore Patient App",
    Author = "IntelliMed",
    Website = "http://intellimed.eu",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = "OrganiMedCore.PatientApp",
    Name = "OrganiMedCore Patient App",
    Description = "AGP+ feature for the patient users.",
    Category = "IntelliMed",
    Dependencies = new[]
    {
        "IntelliMed.Core",
        "OrganiMedCore.Login.Api"
    }
)]