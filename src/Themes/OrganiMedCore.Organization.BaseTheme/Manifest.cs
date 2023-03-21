using OrchardCore.DisplayManagement.Manifest;

[assembly: Theme(
    Name = "Organization Base Theme",
    Author = "IntelliMed",
    Website = "http://intellimed.hu",
    Version = "1.0.0",
    Description = "Base theme for organizations.",
    BaseTheme = "OrganiMedCore.InformationOrientedTheme",
    Dependencies = new string[]
    {
        "OrchardCore.Admin",
        "OrchardCore.Menu"
    }
)]