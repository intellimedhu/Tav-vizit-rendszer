using IntelliMed.Core.Extensions;
using System;
using Xunit;

namespace IntelliMedCore.Tests
{
    public class HttpExtensionsTests
    {
        [Fact]
        public void AppendQueryParams_Empty_ShouldReturnTheSame()
        {
            var uriBuilder = new UriBuilder();
            var result = uriBuilder.AppendQueryParams();
            Assert.Equal(uriBuilder, result);
        }

        [Fact]
        public void AppendQueryParams_Null_ShouldReturnTheSame()
        {
            var uriBuilder = new UriBuilder();
            var result = uriBuilder.AppendQueryParams(null);
            Assert.Equal(uriBuilder, result);
        }

        [Fact]
        public void AppendQueryParams_OddNumOfParams_ShouldThrow_ArgumentException()
        {
            var uriBuilder = new UriBuilder();
            Assert.Throws<ArgumentException>(() => uriBuilder.AppendQueryParams("time", "12:00", "x"));
        }

        [Fact]
        public void AppendQueryParams_ShouldAppend()
        {
            var uriBuilder = new UriBuilder("http://intellimed.hu/")
            {
                Port = -1
            };
            uriBuilder.AppendQueryParams("q", "search");

            Assert.Equal("http://intellimed.hu/?q=search", uriBuilder.ToString());
        }
    }
}
