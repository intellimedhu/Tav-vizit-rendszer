using System;

namespace OrganiMedCore.Core.Exceptions
{
    public class ManagerUnavailableException : Exception
    {
        public ManagerUnavailableException() { }

        public ManagerUnavailableException(string message) : base(message) { }

        public ManagerUnavailableException(string message, Exception innerException) : base(message, innerException) { }
    }
}
