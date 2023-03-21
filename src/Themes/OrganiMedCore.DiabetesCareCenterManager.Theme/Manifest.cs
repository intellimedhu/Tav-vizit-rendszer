using OrchardCore.DisplayManagement.Manifest;

[assembly: Theme(
    Name = "OrganiMedCore Diabetes Care Center Manager Theme",
    Author = "IntelliMed",
    Website = "http://intellimed.hu",
    Version = "1.0.0",
    Description = "Theme for OrganiMedCore Diabetes Care Center Manager.",
    BaseTheme = "OrganiMedCore.InformationOrientedTheme",
    Dependencies = new string[]
    {
        "OrchardCore.Admin",
        "OrchardCore.Menu"
    }
)]