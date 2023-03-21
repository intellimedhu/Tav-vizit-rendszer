using System;

namespace OrganiMedCore.Core.Exceptions
{
    public class OrganizationNotFoundException : Exception
    {
        public OrganizationNotFoundException() { }

        public OrganizationNotFoundException(string message) : base(message) { }

        public OrganizationNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
