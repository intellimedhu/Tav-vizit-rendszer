using System;

namespace OrganiMedCore.Core.Exceptions
{
    public class EVisitOrganizationUserProfileNotFoundException : Exception
    {
        public EVisitOrganizationUserProfileNotFoundException() { }

        public EVisitOrganizationUserProfileNotFoundException(string message) : base(message) { }

        public EVisitOrganizationUserProfileNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
