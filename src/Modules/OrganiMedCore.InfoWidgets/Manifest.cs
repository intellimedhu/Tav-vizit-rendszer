using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrganiMedCore InfoWidgets",
    Author = "IntelliMed",
    Website = "http://intellimed.eu",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = "OrganiMedCore.InfoWidgets",
    Name = "OrganiMedCore InfoWidgets",
    Description = "Contains features for displaying information widgets",
    Category = "IntelliMed",
    Dependencies = new[]
    {
        "OrchardCore.Module.Targets",
        "OrchardCore.Settings",
        "OrchardCore.ContentManagement",
        "OrchardCore.ContentFields",
        "OrchardCore.Contents",
        "OrchardCore.ContentTypes",
        "OrchardCore.Flows",
        "OrchardCore.Html",
        "OrchardCore.Layers",
        "OrchardCore.Liquid",
        "OrchardCore.Lists",
        "OrchardCore.Title",
        "OrchardCore.Templates",
        "OrchardCore.Widgets",
        "OrganiMedCore.Bootstrap"
    }
)]