using IntelliMed.Core.Extensions;
using IntelliMed.Core.Http;
using IntelliMed.DokiNetIntegration.Extensions;
using IntelliMed.DokiNetIntegration.Http;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Settings;
using OrchardCore.Entities;
using OrchardCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IntelliMed.DokiNetIntegration.Services
{
    public class DokiNetService : IDokiNetService
    {
        private readonly IHttpRequestHandler _requestHandler;
        private readonly ISiteService _siteService;

        private const string MemberByLogin = "webapiservices/api/memberaccount/get-dokinetmembers-by-login";
        private const string MemberByNonce = "webapiservices/api/memberaccount/get-dokinetmember-by-webid/{0}";
        private const string MembersByMemberRightIds = "webapiservices/api/memberaccount/get-dokinetmembers-by-memberrightidlist";
        private const string SearchMemberByName = "webapiservices/api/memberaccount/get-dokinetmembers-by-name/{0}/{1}";
        private const string SaveMemberData = "webapiservices/api/member-registration/save-memberdata";
        private const string WatchMembershipUrl = "webapiservices/api/memberaccount/get-dokinetmembers-by-modification";
        private const string PreSharedKey = nameof(PreSharedKey);


        public DokiNetService(IHttpRequestHandler requestHandler, ISiteService siteService)
        {
            _requestHandler = requestHandler;
            _siteService = siteService;
        }


        public async Task<T> GetDokiNetMemberById<T>(int memberRightId) where T : DokiNetMember
        {
            var response = await SendDokiNetRequestAsync<T>(MembersByMemberRightIds, HttpMethod.Post, new[] { memberRightId });

            return response.Members.FirstOrDefault();
        }

        public async Task<IEnumerable<T>> GetDokiNetMembersByIds<T>(IEnumerable<int> memberRightIds) where T : DokiNetMember
        {
            var response = await SendDokiNetRequestAsync<T>(MembersByMemberRightIds, HttpMethod.Post, memberRightIds);

            return response.Members;
        }

        public async Task<T> GetDokiNetMemberByLoginAsync<T>(string username, string password) where T : DokiNetMember
        {
            var response = await SendDokiNetRequestAsync<T>(MemberByLogin, HttpMethod.Post, new
            {
                UserName = username,
                Password = password
            });

            return response.Members.FirstOrDefault();
        }

        public async Task<T> GetDokiNetMemberByNonce<T>(string nonce) where T : DokiNetMember
        {
            var response = await SendDokiNetRequestAsync<T>(string.Format(MemberByNonce, nonce.Trim()));

            return response.Members.FirstOrDefault();
        }

        public async Task<IEnumerable<T>> SearchDokiNetMemberByName<T>(string name) where T : DokiNetMember
        {
            var settings = await GetDokiNetSettingsAsync();

            var response = await SendDokiNetRequestAsync<T>(
                string.Format(SearchMemberByName, settings.MaxMembersCountPerRequest, name.Trim()),
                settings: settings);

            return response.Members;
        }

        public async Task SaveMemberDataAnsyc(int memberId, IEnumerable<MemberData> values)
        {
            values.ThrowIfNull();

            var settings = await GetDokiNetSettingsAsync();
            ThrowIfSettingsEmpty(settings);

            var response = await _requestHandler.SendRequestAsync(new HttpRequestContext()
            {
                AuthorizationScheme = PreSharedKey,
                AuthorizationParameters = settings.GetAuthorization(),
                RequestUri = new UriBuilder(settings.DokiNetBaseUrl) { Port = -1, Path = SaveMemberData }.Uri,
                Method = HttpMethod.Post,
                Content = new SaveMemberDataRequest()
                {
                    MemberId = memberId,
                    Values = values
                }
            });

            response.EnsureSuccessStatusCode();
        }

        public async Task<MembershipWatchResponse> WatchMembershipAsync(DateTime lastCheckDateUtc)
        {
            var settings = await GetDokiNetSettingsAsync();
            ThrowIfSettingsEmpty(settings);

            return await _requestHandler.SendRequestAsync<MembershipWatchResponse>(new HttpRequestContext()
            {
                AuthorizationScheme = PreSharedKey,
                AuthorizationParameters = settings.GetAuthorization(),
                Method = HttpMethod.Get,
                RequestUri = new UriBuilder(settings.DokiNetBaseUrl)
                {
                    Port = -1,
                    Path = WatchMembershipUrl
                }
                .AppendQueryParams("date", lastCheckDateUtc.ToString("yyyy-MM-ddTHH:mm:ss.FFFF"))
                .Uri
            });
        }


        private async Task<DokiNetMemberResponse<T>> SendDokiNetRequestAsync<T>(
            string uriPath,
            HttpMethod method = null,
            object content = null,
            DokiNetSettings settings = null)
            where T : DokiNetMember
        {
            if (settings == null)
            {
                settings = await GetDokiNetSettingsAsync();
            }

            ThrowIfSettingsEmpty(settings);

            var builder = new UriBuilder(settings.DokiNetBaseUrl)
            {
                Path = uriPath,
                Port = -1
            };

            return await _requestHandler.SendRequestAsync<DokiNetMemberResponse<T>>(new HttpRequestContext()
            {
                AuthorizationScheme = PreSharedKey,
                AuthorizationParameters = settings.GetAuthorization(),
                RequestUri = builder.Uri,
                Method = method ?? HttpMethod.Get,
                Content = content
            });
        }

        private void ThrowIfSettingsEmpty(DokiNetSettings settings)
        {
            settings.ThrowIfNull();
            settings.DokiNetBaseUrl.ThrowIfNull();
            settings.PreSharedKey.ThrowIfNull();
            settings.SocietyId.ThrowIfNull();
        }

        private async Task<DokiNetSettings> GetDokiNetSettingsAsync()
            => (await _siteService.GetSiteSettingsAsync()).As<DokiNetSettings>();
    }
}
