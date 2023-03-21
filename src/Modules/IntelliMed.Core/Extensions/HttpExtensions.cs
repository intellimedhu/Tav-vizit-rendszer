using System;
using System.Web;

namespace IntelliMed.Core.Extensions
{
    public static class HttpExtensions
    {
        public static UriBuilder AppendQueryParams(this UriBuilder uriBuilder, params string[] queryParams)
        {
            if (queryParams == null || queryParams.Length == 0)
            {
                return uriBuilder;
            }

            if (queryParams.Length % 2 == 1)
            {
                throw new ArgumentException("Invalid number of parameters.");
            }

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            for (var i = 0; i < queryParams.Length; i += 2)
            {
                query[queryParams[i]] = queryParams[i + 1];
            }

            uriBuilder.Query = query.ToString();

            return uriBuilder;
        }
    }
}
