using System;

namespace OrganiMedCore.DiabetesCareCenter.Core.Exceptions
{
    public class ColleagueException : Exception
    {
        public ColleagueException()
        {
        }

        public ColleagueException(string message) : base(message)
        {
        }
    }
}
