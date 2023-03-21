namespace OrganiMedCore.Manager.ViewModels
{
    public class OrganizationLinkViewModel
    {
        public string Name { get; internal set; }

        public string Hostname { get; set; }

        public string UrlPrefix { get; set; }

        public bool IsActivated { get; internal set; }
    }
}
