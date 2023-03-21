using OrganiMedCore.Email.Indexes;
using OrganiMedCore.Email.Migrations.Schema;
using OrganiMedCore.Email.Models;
using OrganiMedCore.Email.Services;
using OrganiMedCore.Testing.Core;
using OrganiMedCore.Testing.Core.MockServices;
using OrganiMedCore.Testing.Core.SchemaBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using YesSql;

namespace OrganiMedCore.Email.Tests
{
    public class EmailNotificationDataServiceTests
    {
        [Fact]
        public async Task QueueAsync_ShouldQueue()
        {
            var scheduledDateUtc = DateTime.UtcNow.AddHours(-8);
            var email = new EmailNotification()
            {
                ScheduledSendDate = scheduledDateUtc,
                Bcc = new HashSet<string>() { "b@c.c" },
                Cc = new HashSet<string>() { "c@c.c" },
                Data = new Tuple<int, int>(1, 2),
                TemplateId = "T1",
                To = new HashSet<string>() { "t@t1.t", "t@t2.t" }
            };

            await RequestSessionsAsync(
                session =>
                {
                    new EmailNotificationDataService(null, session).QueueAsync(email);

                    return Task.CompletedTask;
                },
                async session =>
                {
                    var persistentEmail = await session.Query<EmailNotification>().FirstOrDefaultAsync();

                    Assert.NotEqual(default(Guid), email.NotificationId);
                    Assert.NotEqual(default(Guid), persistentEmail.NotificationId);
                    Assert.Equal(email.NotificationId, persistentEmail.NotificationId);
                    Assert.Equal(email.Bcc, persistentEmail.Bcc);
                    Assert.Equal(email.Cc, persistentEmail.Cc);
                    Assert.Equal(email.Data, persistentEmail.Data);
                    Assert.Equal(scheduledDateUtc, persistentEmail.ScheduledSendDate);
                    Assert.Equal(email.TemplateId, persistentEmail.TemplateId);
                    Assert.Equal(email.To, persistentEmail.To);
                    Assert.Null(persistentEmail.SentOn);
                });
        }

        [Fact]
        public async Task DequeueAsync_ShouldScheduledBeOnTop()
        {
            var e13 = new EmailNotification() { NotificationId = Guid.NewGuid(), ScheduledSendDate = DateTime.UtcNow.AddMinutes(-11.2) };
            var e4 = new EmailNotification() { NotificationId = Guid.NewGuid(), ScheduledSendDate = DateTime.UtcNow.AddMinutes(-10) };
            var e1 = new EmailNotification() { NotificationId = Guid.NewGuid(), ScheduledSendDate = DateTime.UtcNow.AddMinutes(-5) };
            var e3 = new EmailNotification() { NotificationId = Guid.NewGuid(), ScheduledSendDate = DateTime.UtcNow.AddMinutes(-3.3) };
            var e5 = new EmailNotification() { NotificationId = Guid.NewGuid(), ScheduledSendDate = DateTime.UtcNow.AddMinutes(-2.5) };
            var e9 = new EmailNotification() { NotificationId = Guid.NewGuid(), ScheduledSendDate = DateTime.UtcNow.AddMinutes(-1.7) };
            var e7 = new EmailNotification() { NotificationId = Guid.NewGuid(), ScheduledSendDate = DateTime.UtcNow.AddMinutes(-1) };
            var e11 = new EmailNotification() { NotificationId = Guid.NewGuid(), ScheduledSendDate = DateTime.UtcNow.AddMinutes(-.5) };

            var e2 = new EmailNotification() { NotificationId = Guid.NewGuid(), TemplateId = "T1", Data = new { A = "a" } };
            var e6 = new EmailNotification() { NotificationId = Guid.NewGuid() };
            var e8 = new EmailNotification() { NotificationId = Guid.NewGuid() };
            var e10 = new EmailNotification() { NotificationId = Guid.NewGuid() };
            var e12 = new EmailNotification() { NotificationId = Guid.NewGuid() };


            await RequestSessionsAsync(
                session =>
                {
                    var service = new EmailNotificationDataService(null, session);

                    service.QueueAsync(e1);
                    service.QueueAsync(e2);
                    service.QueueAsync(e3);
                    service.QueueAsync(e4);
                    service.QueueAsync(e5);
                    service.QueueAsync(e6);
                    service.QueueAsync(e7);
                    service.QueueAsync(e8);
                    service.QueueAsync(e9);
                    service.QueueAsync(e10);
                    service.QueueAsync(e11);
                    service.QueueAsync(e12);
                    service.QueueAsync(e13);

                    return Task.CompletedTask;
                },
                async session =>
                {
                    var service = new EmailNotificationDataService(new ClockMock(), session);

                    var emails = await service.DequeueAsync(10);

                    Assert.Equal(10, emails.Count());

                    Assert.Equal(e13.NotificationId, emails.First().NotificationId);
                    Assert.Equal(e4.NotificationId, emails.Skip(1).First().NotificationId);
                    Assert.Equal(e1.NotificationId, emails.Skip(2).First().NotificationId);
                    Assert.Equal(e3.NotificationId, emails.Skip(3).First().NotificationId);
                    Assert.Equal(e5.NotificationId, emails.Skip(4).First().NotificationId);
                    Assert.Equal(e9.NotificationId, emails.Skip(5).First().NotificationId);
                    Assert.Equal(e7.NotificationId, emails.Skip(6).First().NotificationId);
                    Assert.Equal(e11.NotificationId, emails.Skip(7).First().NotificationId);
                    Assert.Equal(e2.NotificationId, emails.Skip(8).First().NotificationId);
                    Assert.Equal(e6.NotificationId, emails.Skip(9).First().NotificationId);
                });
        }

        [Fact]
        public async Task MarkAsSentAsync_ShouldBeMarkedAsSent()
        {
            var scheduledDateUtc = DateTime.UtcNow.AddMinutes(-17);
            var email = new EmailNotification()
            {
                ScheduledSendDate = scheduledDateUtc
            };

            await RequestSessionsAsync(
               session =>
               {
                   new EmailNotificationDataService(null, session).QueueAsync(email);

                   return Task.CompletedTask;
               },
               async session =>
               {
                   await new EmailNotificationDataService(new ClockMock(), session)
                        .MarkAsSentAsync(email.NotificationId);
               },
               async session =>
               {
                   var persistentEmail = await session.Query<EmailNotification>().FirstOrDefaultAsync();

                   Assert.Equal(email.NotificationId, persistentEmail.NotificationId);
                   Assert.NotNull(persistentEmail.SentOn);
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
                        EmailNotificationSchemaBuilder.Build(schemaBuilder);
                    },
                    store =>
                    {
                        store.RegisterIndexes<EmailNotificationIndexProvider>();
                    });
                await sessionHandler.RequestSessionsAsync(sessions);
            }
        }
    }
}
