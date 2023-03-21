using Microsoft.Extensions.Caching.Memory;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Environment.Cache;
using OrganiMedCore.Email.Models;
using OrganiMedCore.Email.Services;
using OrganiMedCore.Email.ViewModels;
using OrganiMedCore.Testing.Core;
using OrganiMedCore.Testing.Core.SchemaBuilders;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using YesSql;

namespace OrganiMedCore.Email.Tests
{
    public class EmailTokenizerDataServiceTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task FindEmailTemplateByIdAsync_ShouldFind(bool shouldFindById)
        {
            var service = new EmailTokenizerDataService(
                new[]
                {
                    new EmailTemplateProvider(shouldFindById)
                },
                null,
                null,
                null);

            var template = await service.FindEmailTemplateByIdAsync("t");

            Assert.Equal(shouldFindById, template != null);
        }

        [Theory]
        [InlineData("a,b,c,d,e,f,g")]
        [InlineData("")]
        public async Task GetEmailTemplatesAsync_ShouldCollectAll(string templateIdsConcatenated)
        {
            var templateIds = templateIdsConcatenated.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var emailTemplateProviders = templateIds.Select(t =>
                new EmailTemplateProvider(new[] { new EmailTemplate() { Id = t } }));

            var service = new EmailTokenizerDataService(emailTemplateProviders, null, null, null);

            var templates = await service.GetEmailTemplatesAsync();

            Assert.Equal(templateIds.Length, templates.Count());
            Assert.All(templates, template => templateIds.Any(t => t == template.Id));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        public async Task GetTokenizedEmailByTemplateIdAsync_ShouldReturn(int count)
        {
            var emails = new TokenizedEmail[count];
            for (var i = 0; i < count; i++)
            {
                emails[i] = new TokenizedEmail()
                {
                    TemplateId = "template" + i,
                    RawBody = "raw" + i
                };
            }

            var results = new List<TokenizedEmail>();

            await RequestSessionsAsync(
                session =>
                {
                    var document = new TokenizedEmailDocument();

                    foreach (var email in emails)
                    {
                        document.TokenizedEmails.Add(email);
                    }

                    session.Save(document);

                    return Task.CompletedTask;
                },
                async session =>
                {
                    var service = new EmailTokenizerDataService(null, GetCache(), session, GetSignal());
                    foreach (var email in emails)
                    {
                        results.Add(await service.GetTokenizedEmailByTemplateIdAsync(email.TemplateId));
                    }
                });

            Assert.Equal(count, results.Count);
            Assert.All(emails, email => results.Any(x => x.RawBody.Equals(email.RawBody) && x.TemplateId.Equals(email.TemplateId)));
        }

        [Fact]
        public async Task SaveTokenizedEmailAsync_ShouldThrow_ArgumentNullException()
        {
            var service = new EmailTokenizerDataService(null, null, null, null);

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.SaveTokenizedEmailAsync(null));
        }

        [Theory]
        [InlineData(null, "body", "t2", typeof(ArgumentNullException))]
        [InlineData("", "body", "t2", typeof(ArgumentException))]
        [InlineData("subj.", null, "t2", typeof(ArgumentNullException))]
        [InlineData("subj.", "", "t2", typeof(ArgumentException))]
        [InlineData("subj.", "body", null, typeof(ArgumentNullException))]
        [InlineData("subj.", "body", "", typeof(ArgumentException))]
        public async Task SaveTokenizedEmailAsync_ShouldThrow_AsExpected(string subject, string rawBody, string templateId, Type expectedException)
        {
            var service = new EmailTokenizerDataService(null, null, null, null);

            await Assert.ThrowsAsync(expectedException,
                () => service.SaveTokenizedEmailAsync(new TokenizedEmailViewModel()
                {
                    Subject = subject,
                    RawBody = rawBody,
                    TemplateId = templateId
                }));
        }

        [Fact]
        public async Task SaveTokenizedEmailAsync_ShouldSave()
        {
            TokenizedEmail result = null;
            var rawBody = "raw body";
            var subject = "sample subj.";
            var templateId = "t01";

            await RequestSessionsAsync(
                async session =>
                {
                    var service = new EmailTokenizerDataService(
                        new[]
                        {
                            new EmailTemplateProvider(true)
                        },
                        GetCache(),
                        session,
                        GetSignal());

                    await service.SaveTokenizedEmailAsync(new TokenizedEmailViewModel()
                    {
                        RawBody = rawBody,
                        Subject = subject,
                        TemplateId = templateId
                    });
                },
                async session =>
                {
                    result = (await session.Query<TokenizedEmailDocument>().FirstOrDefaultAsync())?.TokenizedEmails.FirstOrDefault();
                });

            Assert.NotNull(result);
            Assert.Equal(rawBody, result.RawBody);
            Assert.Equal(subject, result.Subject);
            Assert.Equal(templateId, result.TemplateId);
        }

        [Fact]
        public async Task SaveMany_ShouldSaveAll()
        {
            var viewModels = new[]
            {
                new TokenizedEmailViewModel() { TemplateId = "A", Subject = "S1", RawBody = "RB1" },
                new TokenizedEmailViewModel() { TemplateId = "B", Subject = "S2", RawBody = "RB2" },
                new TokenizedEmailViewModel() { TemplateId = "C", Subject = "S3", RawBody = "RB3" },
                new TokenizedEmailViewModel() { TemplateId = "D", Subject = "S4", RawBody = "RB4" },
                new TokenizedEmailViewModel() { TemplateId = "E", Subject = "S5", RawBody = "RB5" }
            };

            await RequestSessionsAsync(
                async session =>
                {
                    var service = new EmailTokenizerDataService(null, GetCache(), session, GetSignal());
                    await service.SaveManyTokenizedEmailsAsync(viewModels);
                },
                async session =>
                {
                    var document = await session.Query<TokenizedEmailDocument>().FirstOrDefaultAsync();
                    Assert.All(document.TokenizedEmails, t => viewModels.Any(x =>
                        x.TemplateId == t.TemplateId &&
                        x.Subject == t.Subject &&
                        x.RawBody == t.RawBody));
                });
        }

        [Fact]
        public async Task SaveMany_AndSaveAgain_ShouldSaveAll()
        {
            var viewModels = new[]
            {
                new TokenizedEmailViewModel() { TemplateId = "X", Subject = "XX1", RawBody = "X1" },
                new TokenizedEmailViewModel() { TemplateId = "Y", Subject = "XY2", RawBody = "Y2" },
                new TokenizedEmailViewModel() { TemplateId = "Z", Subject = "XZ3", RawBody = "Z3" }
            };

            await RequestSessionsAsync(
                async session =>
                {
                    var service = new EmailTokenizerDataService(null, GetCache(), session, GetSignal());
                    await service.SaveManyTokenizedEmailsAsync(viewModels);
                },
                async session =>
                {
                    var service = new EmailTokenizerDataService(null, GetCache(), session, GetSignal());
                    await service.SaveManyTokenizedEmailsAsync(viewModels);
                },
                async session =>
                {
                    var document = await session.Query<TokenizedEmailDocument>().FirstOrDefaultAsync();
                    Assert.True(document.TokenizedEmails.Count == viewModels.Length);
                });
        }


        private async Task RequestSessionsAsync(params Func<ISession, Task>[] sessions)
        {
            using (var sessionHandler = new YesSqlSessionHandler())
            {
                await sessionHandler.InitializeAsync();
                await sessionHandler.RegisterSchemaAndIndexes(
                    schemaBuilder =>
                    {
                        ContentItemSchemaBuilder.Build(schemaBuilder);
                    },
                    store =>
                    {
                        store.RegisterIndexes<ContentItemIndexProvider>();
                    });
                await sessionHandler.RequestSessionsAsync(sessions);
            }
        }

        private IMemoryCache GetCache() => new MemoryCache(new MemoryCacheOptions());

        private ISignal GetSignal() => new Signal();
    }

    class EmailTemplateProvider : IEmailTemplateProvider
    {
        private readonly bool _shouldFindById;
        private readonly IEnumerable<EmailTemplate> _expectedResult;


        public EmailTemplateProvider(bool shouldFindById)
        {
            _shouldFindById = shouldFindById;
        }

        public EmailTemplateProvider(IEnumerable<EmailTemplate> expectedResult)
        {
            _expectedResult = expectedResult;
        }


        public Task<EmailTemplate> GetEmailTemplateByIdAsync(string templateId)
        {
            EmailTemplate result = null;
            if (_shouldFindById)
            {
                result = new EmailTemplate() { Id = templateId };

            }

            return Task.FromResult(result);
        }

        public Task<IEnumerable<EmailTemplate>> GetEmailTemplatesAsync()
            => Task.FromResult(_expectedResult);

        [ExcludeFromCodeCoverage]
        public Task<string> ProcessAsync(string templateId, object data, string rawBody)
            => throw new NotImplementedException();
    }
}
