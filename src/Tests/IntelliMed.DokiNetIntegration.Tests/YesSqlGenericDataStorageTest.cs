using OrganiMedCore.Testing.Core;
using System;
using System.Threading.Tasks;
using Xunit;
using YesSql;

namespace IntelliMed.DokiNetIntegration.Tests
{
    public class YesSqlGenericDataStorageTest
    {
        [Fact]
        public async Task TestGeneric_ShouldStoreAndReturn()
        {
            var dateOfA = DateTime.UtcNow;
            var stringOfB = "foo-bar";
            var numA = 0xA;
            var numB = 99.2342012;

            await RequestSessionsAsync(
                session =>
                {
                    var containerA = new Container<A>() { Data = new A() { Number = numA, DateOfA = dateOfA } };
                    var containerB = new Container<B>() { Data = new B() { Number = numB, StringOfB = stringOfB } };

                    session.Save(containerA);
                    session.Save(containerB);

                    return Task.CompletedTask;
                },
                async session =>
                {
                    var a = await session.Query<Container<A>>().FirstOrDefaultAsync();

                    Assert.NotNull(a);
                    Assert.Equal(numA, a.Data.Number);
                    Assert.Equal(dateOfA, a.Data.DateOfA);
                },
                async session =>
                {
                    var b = await session.Query<Container<B>>().FirstOrDefaultAsync();

                    Assert.NotNull(b);
                    Assert.Equal(numB, b.Data.Number);
                    Assert.Equal(stringOfB, b.Data.StringOfB);
                });
        }


        private async Task RequestSessionsAsync(params Func<ISession, Task>[] sessions)
        {
            using (var sessionHandler = new YesSqlSessionHandler())
            {
                await sessionHandler.InitializeAsync();
                await sessionHandler.RequestSessionsAsync(sessions);
            }
        }


        class Container<T> where T : new()
        {
            public T Data { get; set; }
        }

        class A
        {
            public int Number { get; set; }

            public DateTime DateOfA { get; set; }
        }

        class B
        {
            public double Number { get; set; }

            public string StringOfB { get; set; }
        }
    }
}
