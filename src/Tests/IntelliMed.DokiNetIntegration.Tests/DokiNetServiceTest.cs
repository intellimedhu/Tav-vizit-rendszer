using IntelliMed.Core.Http;
using IntelliMed.DokiNetIntegration.Http;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using IntelliMed.DokiNetIntegration.Settings;
using OrchardCore.Entities;
using OrganiMedCore.Testing.Core.MockServices;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace IntelliMed.DokiNetIntegration.Tests
{
    public class DokiNetServiceTest
    {
        [Fact]
        public async Task GetDokiNetMemberById_ShouldReturnMember()
        {
            const int memberId = 11223344;
            var httpResponse = new DokiNetMemberResponse<DokiNetMember>()
            {
                Members = new[]
                {
                    new DokiNetMember()
                    {
                        MemberId = memberId
                    }
                }
            };

            var service = new DokiNetService(new HttpRequestHandlerMock(httpResponse), await GetSiteService());

            var dokiNetMember = await service.GetDokiNetMemberById<DokiNetMember>(memberId);

            Assert.Equal(memberId, dokiNetMember.MemberId);
        }

        [Fact]
        public async Task GetDokiNetMemberById_ShouldThrow()
        {
            const int memberId = 11223344;
            var httpResponse = new DokiNetMemberResponse<DokiNetMember>()
            {
                Members = new DokiNetMember[] { }
            };

            var service = new DokiNetService(new HttpRequestHandlerMock(httpResponse), await GetSiteService());
            var dokiNetMember = await service.GetDokiNetMemberById<DokiNetMember>(memberId);

            Assert.Null(dokiNetMember);
        }

        [Fact]
        public async Task GetDokiNetMemberByLoginAsync_ShouldLoginSuccess()
        {
            const int memberId = 11223344;
            var httpResponse = new DokiNetMemberResponse<DokiNetMember>()
            {
                Members = new[]
                {
                    new DokiNetMember()
                    {
                        MemberId = memberId,
                        WebId = "WEBID"
                    }
                }
            };

            var service = new DokiNetService(new HttpRequestHandlerMock(httpResponse), await GetSiteService());

            var dokiNetMember = await service.GetDokiNetMemberByLoginAsync<DokiNetMember>("a", "b");

            Assert.NotNull(dokiNetMember);
            Assert.False(string.IsNullOrEmpty(dokiNetMember.WebId));
        }

        [Fact]
        public async Task GetDokiNetMemberByLoginAsync_ShouldLoginFail()
        {
            var httpResponse = new DokiNetMemberResponse<DokiNetMember>()
            {
                Members = new DokiNetMember[0]
            };

            var service = new DokiNetService(new HttpRequestHandlerMock(httpResponse), await GetSiteService());

            Assert.Null(await service.GetDokiNetMemberByLoginAsync<DokiNetMember>("a", "b"));
        }

        [Fact]
        public async Task GetDokiNetMembersByIds()
        {
            var ids = new[] { 400187, 400571, 400697, 400874 };
            var httpResponse = new DokiNetMemberResponse<DokiNetMember>()
            {
                Members = new[]
                {
                    new DokiNetMember()
                    {
                        MemberId = ids[0]
                    },
                    new DokiNetMember()
                    {
                        MemberId = ids[1]
                    },
                    new DokiNetMember()
                    {
                        MemberId = ids[2]
                    },
                    new DokiNetMember()
                    {
                        MemberId = ids[3]
                    }
                }
            };

            var service = new DokiNetService(new HttpRequestHandlerMock(httpResponse), await GetSiteService());

            var members = await service.GetDokiNetMembersByIds<DokiNetMember>(ids);

            Assert.NotNull(members);
            Assert.All(members, m => ids.Contains(m.MemberRightId));
        }

        [Fact]
        public async Task SearchDokiNetMemberByName()
        {
            var httpResponse = new DokiNetMemberResponse<DokiNetMember>()
            {
                Members = new[]
                {
                    new DokiNetMember()
                    {
                        MemberId = 1
                    },
                    new DokiNetMember()
                    {
                        MemberId = 2
                    }
                }
            };

            var service = new DokiNetService(new HttpRequestHandlerMock(httpResponse), await GetSiteService());
            var members = await service.SearchDokiNetMemberByName<DokiNetMember>("a");

            Assert.Equal(httpResponse.Members.Count(), members.Count());
        }

        [Fact]
        public async Task GetDokiNetMemberByNonce()
        {
            const string webId = nameof(webId);

            var httpResponse = new DokiNetMemberResponse<DokiNetMember>()
            {
                Members = new[]
                {
                    new DokiNetMember()
                    {
                        MemberId = 1,
                        WebId=webId
                    }
                }
            };

            var service = new DokiNetService(new HttpRequestHandlerMock(httpResponse), await GetSiteService());

            var member = await service.GetDokiNetMemberByNonce<DokiNetMember>(webId);

            Assert.NotNull(member);
            Assert.False(string.IsNullOrEmpty(member.WebId));
        }

        [Fact]
        public async Task SaveMemberDataAnsyc_WithLicense()
        {
            var requestHandlerMock = new HttpRequestHandlerMock(null);

            var service = new DokiNetService(requestHandlerMock, await GetSiteService());

            const int memberId = 159;
            const string code = nameof(code);
            const string value = nameof(value);
            await service.SaveMemberDataAnsyc(
                memberId,
                new MemberData[]
                {
                    new MemberData() { DataCode = code, DataValue = value }
                });

            Assert.Equal(memberId, requestHandlerMock.MemberId);
            Assert.Equal(code, requestHandlerMock.Code);
            Assert.Equal(value, requestHandlerMock.Value);
        }

        [Fact]
        public async Task WatchMembership()
        {
            var httpResponse = new MembershipWatchResponse()
            {
                LastCheckDate = DateTime.Parse("1964-04-14T15:29:33.237"),
                MemberRightIds = new[] { 2, 3, 4 }
            };

            var service = new DokiNetService(new HttpRequestHandlerMock(httpResponse), await GetSiteService());
            var response = await service.WatchMembershipAsync(DateTime.Now.Date);

            Assert.Equal(httpResponse.LastCheckDate, response.LastCheckDate);
            Assert.Equal(httpResponse.MemberRightIds, response.MemberRightIds);
        }


        private async Task<SiteServiceMock> GetSiteService(int? maxMembersCountPerRequest = null)
        {
            var siteService = new SiteServiceMock();
            var settings = await siteService.GetSiteSettingsAsync();

            settings.Alter<DokiNetSettings>(nameof(DokiNetSettings), x =>
            {
                x.DokiNetBaseUrl = "http://desktop-fg/";
                x.PreSharedKey = "x85J3zK7QAriMTUbH80L";
                x.SocietyId = 20;
                x.MaxMembersCountPerRequest = maxMembersCountPerRequest ?? 10;
            });

            await siteService.UpdateSiteSettingsAsync(settings);

            return siteService;
        }


        private class HttpRequestHandlerMock : IHttpRequestHandler
        {
            private readonly object _response;


            public int MemberId { get; private set; }

            public string Code { get; private set; }

            public string Value { get; private set; }


            public HttpRequestHandlerMock(object response)
            {
                _response = response;
            }


            public Task<HttpResponseMessage> SendRequestAsync(HttpRequestContext context)
            {
                var data = context.Content as SaveMemberDataRequest;

                MemberId = data.MemberId;
                Code = data.Values.First().DataCode;
                Value = data.Values.First().DataValue;

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }


            [ExcludeFromCodeCoverage]
            public Task<T> SendRequestAsync<T>(HttpRequestContext context) where T : new()
            {
                if (_response is T t)
                {
                    return Task.FromResult(t);
                }

                throw new ArgumentException();
            }
        }
    }
}
