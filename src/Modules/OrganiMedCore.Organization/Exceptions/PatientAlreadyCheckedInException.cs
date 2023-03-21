using System;

namespace OrganiMedCore.Organization.Exceptions
{
    public class PatientAlreadyCheckedInException : Exception
    {
        public PatientAlreadyCheckedInException() { }

        public PatientAlreadyCheckedInException(string message) : base(message) { }

        public PatientAlreadyCheckedInException(string message, Exception innerException) : base(message, innerException) { }
    }
}
