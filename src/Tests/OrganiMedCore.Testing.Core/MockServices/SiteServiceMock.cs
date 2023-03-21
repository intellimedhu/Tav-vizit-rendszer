using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using OrchardCore.Settings;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.Testing.Core.MockServices
{
    public class SiteServiceMock : ISiteService
    {
        private ISite _site;

        public IChangeToken ChangeToken => throw new NotImplementedException();


        public SiteServiceMock()
        {
            _site = new MockSiteSettings();
        }


        public Task<ISite> GetSiteSettingsAsync() => Task.FromResult(_site);

        public Task UpdateSiteSettingsAsync(ISite site)
        {
            _site = site;

            return Task.CompletedTask;
        }
    }

    public class MockSiteSettings : ISite
    {
        public string SiteName { get; set; }
        public string SiteSalt { get; set; }
        public string SuperUser { get; set; }
        public string Calendar { get; set; }
        public string TimeZoneId { get; set; }
        public ResourceDebugMode ResourceDebugMode { get; set; }
        public bool UseCdn { get; set; }
        public string CdnBaseUrl { get; set; }
        public int PageSize { get; set; }
        public int MaxPageSize { get; set; }
        public int MaxPagedCount { get; set; }
        public string BaseUrl { get; set; }
        public RouteValueDictionary HomeRoute { get; set; }
        public bool AppendVersion { get; set; }
        public JObject Properties { get; }
        public string PageTitleFormat { get; set; }

        public MockSiteSettings()
        {
            Properties = new JObject();
        }
    }
}
