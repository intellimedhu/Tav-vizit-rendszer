using System;

namespace OrganiMedCore.Organization.Exceptions
{
    public class UserTypeException : Exception
    {
        public UserTypeException() { }

        public UserTypeException(string message) : base(message) { }

        public UserTypeException(string message, Exception innerException) : base(message, innerException) { }
    }
}
