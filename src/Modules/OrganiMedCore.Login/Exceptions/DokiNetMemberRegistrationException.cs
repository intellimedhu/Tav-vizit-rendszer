using System;

namespace OrganiMedCore.Login.Exceptions
{
    public class DokiNetMemberRegistrationException : Exception
    {
        public DokiNetMemberRegistrationException(string message) : base(message)
        {
        }
    }
}
