using System;

namespace OrganiMedCore.Organization.Exceptions
{
    public class OrganizationUnitNotFoundException : Exception
    {
        public OrganizationUnitNotFoundException() { }

        public OrganizationUnitNotFoundException(string message) : base(message) { }

        public OrganizationUnitNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
