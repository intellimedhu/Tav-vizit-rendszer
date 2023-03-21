using System;

namespace OrganiMedCore.DiabetesCareCenter.Core.Exceptions
{
    public class TerritoryException : Exception
    {
        public TerritoryException()
        {
        }

        public TerritoryException(string message) : base(message)
        {
        }
    }
}
