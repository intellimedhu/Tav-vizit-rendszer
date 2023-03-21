using IntelliMed.Core.Extensions;
using IntelliMed.Core.Services;
using Lucene.Net.Documents;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Environment.Shell;
using OrchardCore.Lucene;
using OrganiMedCore.Core.Constants;
using OrganiMedCore.Core.Indexes;
using OrganiMedCore.Core.Models;
using OrganiMedCore.Core.Models.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using YesSql.Services;

namespace OrganiMedCore.Core.Services
{
    public class EVisitPatientProfileService : IEVisitPatientProfileService
    {
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly ShellSettings _shellSettings;


        public EVisitPatientProfileService(
            ISharedDataAccessorService sharedDataAccessorService,
            ShellSettings shellSettings)
        {
            _sharedDataAccessorService = sharedDataAccessorService;
            _shellSettings = shellSettings;
        }


        public async Task<ContentItem> GetByIdentifierAsync(IServiceScope managersServiceScope, string identifierValue, PatientIdentifierTypes identifierType)
        {
            if (identifierType == PatientIdentifierTypes.None || string.IsNullOrEmpty(identifierValue))
            {
                return null;
            }

            var managerSession = managersServiceScope.ServiceProvider.GetRequiredService<ISession>();

            return await managerSession
                .Query<ContentItem, EVisitPatientProfilePartIndex>(x =>
                    x.PatientIdentifierType == identifierType && x.PatientIdentifierValue == identifierValue)
                .LatestAndPublished()
                .FirstOrDefaultAsync();
        }

        public async Task<ContentItem> InitializeAsync(IServiceScope managersServiceScope)
            => await managersServiceScope.ServiceProvider.GetRequiredService<IContentManager>()
                .NewAsync(ContentTypes.EVisitPatientProfile);

        public async Task<ContentItem> GetAsync(IServiceScope managersServiceScope, string contentItemId)
            => await managersServiceScope.ServiceProvider.GetRequiredService<IContentManager>()
                .GetAsync(contentItemId, ContentTypes.EVisitPatientProfile);

        public async Task<ContentItem> GetNewVersionAsync(IServiceScope managersServiceScope, string contentItemId)
            => await managersServiceScope.ServiceProvider.GetRequiredService<IContentManager>()
                .GetNewVersionAsync(contentItemId, ContentTypes.EVisitPatientProfile);

        public async Task<IEnumerable<ContentItem>> SearchAsync(IServiceScope managersServiceScope, EVisitPatientProfileFilter filter, int size)
        {
            var luceneQuerySource = managersServiceScope.ServiceProvider.GetRequiredService<LuceneQuerySource>();

            var must = new List<object>
                    {
                        new { term = new Dictionary<string, string> { { LuceneEntryNames.ContentType, ContentTypes.EVisitPatientProfile } } }
                    };
            if (!string.IsNullOrEmpty(filter.FirstName))
            {
                must.Add(new { match = new Dictionary<string, string> { { LuceneEntryNames.FirstName, filter.FirstName } } });
            }
            if (!string.IsNullOrEmpty(filter.LastName))
            {
                must.Add(new { match = new Dictionary<string, string> { { LuceneEntryNames.LastName, filter.LastName } } });
            }
            if (!string.IsNullOrEmpty(filter.Email))
            {
                must.Add(new { match = new Dictionary<string, string> { { LuceneEntryNames.Email, filter.Email } } });
            }
            if (!string.IsNullOrEmpty(filter.MothersName))
            {
                must.Add(new { match = new Dictionary<string, string> { { LuceneEntryNames.MothersName, filter.MothersName } } });
            }
            if (filter.DateOfBirth.HasValue)
            {
                must.Add(new { match = new Dictionary<string, string> { { LuceneEntryNames.DateOfBirth, DateTools.DateToString(filter.DateOfBirth.Value, DateTools.Resolution.DAY) } } });
            }
            if (!string.IsNullOrEmpty(filter.PatientIdentifierValue))
            {
                must.Add(new { match = new Dictionary<string, string> { { LuceneEntryNames.PatientIdentifierValue, filter.PatientIdentifierValue } } });
            }
            if (filter.OnlyLocalPatinets)
            {
                must.Add(new { match = new Dictionary<string, string> { { LuceneEntryNames.AttendedOrganizationNames, _shellSettings.Name } } });
            }

            #region Template example
            //{
            //    "query": {
            //        "bool": {
            //            "must": [
            //              {
            //            "match": {
            //            "EVisitPatientProfilePart.FirstName": "Albert"
            //            }
            //},
            //        {
            //            "match": {
            //            "EVisitPatientProfilePart.LastName": "Hajdu"
            //            }
            //        },
            //        {
            //           "term" : { "Content.ContentItem.ContentType" : "EVisitPatientProfile" }
            //            }
            //        ]
            //    }
            //    }
            //}
            #endregion

            var template = new
            {
                query = new
                {
                    @bool = new
                    {
                        must
                    }
                },
                size
            };

            var luceneQuery = new LuceneQuery
            {
                Index = Common.EVisitPatientsIndex,
                Template = JsonConvert.SerializeObject(template),
                ReturnContentItems = true
            };
            var result = await luceneQuerySource.ExecuteQueryAsync(luceneQuery, new Dictionary<string, object> { { "size", size } }) as ContentItem[];

            return result.OrderBy(x => x.As<EVisitPatientProfilePart>().LastName).ThenBy(x => x.As<EVisitPatientProfilePart>().FirstName);
        }

        public async Task<IEnumerable<ContentItem>> GetByIds(IServiceScope managersServiceScope, IEnumerable<string> ids)
        {
            var managerSession = managersServiceScope.ServiceProvider.GetRequiredService<ISession>();

            return await managerSession
                 .Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentItemId.IsIn(ids) &&
                    x.ContentType == ContentTypes.EVisitPatientProfile)
                .LatestAndPublished()
                .With<EVisitPatientProfilePartIndex>()
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .ListAsync();
        }
    }
}
