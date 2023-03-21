using System;

namespace OrganiMedCore.Organization.Exceptions
{
    public class PatientNotFoundException : Exception
    {
        public PatientNotFoundException() { }

        public PatientNotFoundException(string message) : base(message) { }

        public PatientNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
