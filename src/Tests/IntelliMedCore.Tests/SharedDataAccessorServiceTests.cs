using IntelliMed.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Scope;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IntelliMedCore.Tests
{
    public class SharedDataAccessorServiceTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(4)]
        public async Task ListShellContexts_ShouldReturnAll(int count)
        {
            var service = new SharedDataAccessorService(new ShellHostMock(count), null, null, null);
            var result = await service.ListShellContextsAsync();
            Assert.Equal(count, result.Count());
        }


        private class ShellHostMock : IShellHost
        {
            private readonly int _mockShellContextsCount;


            public ShellHostMock(int count)
            {
                _mockShellContextsCount = count;
            }


            public IEnumerable<ShellContext> ListShellContexts()
            {
                var result = new List<ShellContext>();
                for (var i = 0; i < _mockShellContextsCount; i++)
                {
                    result.Add(new ShellContext());
                }

                return result;
            }

            [ExcludeFromCodeCoverage]
            public Task<ShellContext> CreateShellContextAsync(ShellSettings settings)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public IEnumerable<ShellSettings> GetAllSettings()
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<ShellContext> GetOrCreateShellContextAsync(ShellSettings settings)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<(IServiceScope Scope, ShellContext ShellContext)> GetScopeAndContextAsync(ShellSettings settings)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<ShellScope> GetScopeAsync(ShellSettings settings)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task InitializeAsync()
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task ReloadShellContextAsync(ShellSettings settings)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public bool TryGetSettings(string name, out ShellSettings settings)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task UpdateShellSettingsAsync(ShellSettings settings)
            {
                throw new NotImplementedException();
            }
        }
    }
}
