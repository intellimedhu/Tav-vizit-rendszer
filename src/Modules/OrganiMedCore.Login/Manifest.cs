using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrganiMedCore Login",
    Author = "IntelliMed",
    Website = "http://intellimed.eu",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = "OrganiMedCore.Login",
    Name = "OrganiMedCore Login",
    Description = "Normal, OrganiMedCore and doki.Net login features for tenants.",
    Category = "IntelliMed",
    Dependencies = new[]
    {
        "OrchardCore.Admin",
        "OrchardCore.Liquid",
        "OrchardCore.Users",
        "OrganiMedCore.Core",
        "IntelliMed.DokiNetIntegration"
    }
)]

[assembly: Feature(
    Id = "OrganiMedCore.Login.Api",
    Name = "OrganiMedCore.Login.Api",
    Description = "Normal, OrganiMedCore and doki.Net login features for tenants. The communication can be done via HTTP requests.",
    Category = "IntelliMed",
    Dependencies = new[]
    {
        "OrchardCore.Admin",
        "OrchardCore.Liquid",
        "OrchardCore.Users",
        "OrganiMedCore.Core",
        "OrganiMedCore.Login",
        "IntelliMed.DokiNetIntegration"
    }
)]

[assembly: Feature(
    Id = "OrganiMedCore.Login.Registration",
    Name = "OrganiMedCore Login Registration",
    Description = "Shared login registration.",
    Category = "IntelliMed",
    Dependencies = new string[]
    {
        "OrganiMedCore.Login",
        "OrchardCore.Users.Registration"
    }
)]

[assembly: Feature(
    Id = "OrganiMedCore.Login.ResetPassword",
    Name = "OrganiMedCore Login ResetPassword",
    Description = "Shared login reset password.",
    Category = "IntelliMed",
    Dependencies = new string[]
    {
        "OrganiMedCore.Login",
        "OrchardCore.Users.ResetPassword"
    }
)]