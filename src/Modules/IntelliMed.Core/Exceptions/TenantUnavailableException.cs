using System;

namespace IntelliMed.Core.Exceptions
{
    public class TenantUnavailableException : Exception
    {
        public TenantUnavailableException() { }

        public TenantUnavailableException(string message) : base(message) { }

        public TenantUnavailableException(string message, Exception innerException) : base(message, innerException) { }
    }
}
