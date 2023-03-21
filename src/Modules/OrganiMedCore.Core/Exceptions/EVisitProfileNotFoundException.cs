using System;

namespace OrganiMedCore.Core.Exceptions
{
    public class EVisitProfileNotFoundException : Exception
    {
        public EVisitProfileNotFoundException() { }

        public EVisitProfileNotFoundException(string message) : base(message) { }

        public EVisitProfileNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
