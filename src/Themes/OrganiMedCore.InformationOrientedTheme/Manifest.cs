using OrchardCore.DisplayManagement.Manifest;

[assembly: Theme(
    Name = "OrganiMedCore Information Oriented Theme",
    Author = "IntelliMed",
    Website = "http://intellimed.hu",
    Version = "1.0.0",
    BaseTheme = "OrganiMedCore.Login.BaseTheme",
    Description = "Common views and styles for themes where it is necessary to be information block(s).",
    Dependencies = new[]
    {
        "OrchardCore.Menu",
        "OrganiMedCore.Navigation"
    }
)]