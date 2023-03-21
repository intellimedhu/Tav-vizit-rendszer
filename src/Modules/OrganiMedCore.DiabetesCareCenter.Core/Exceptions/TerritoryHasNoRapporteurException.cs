namespace OrganiMedCore.DiabetesCareCenter.Core.Exceptions
{
    public class TerritoryHasNoRapporteurException : TerritoryException
    {
        public TerritoryHasNoRapporteurException()
        {
        }

        public TerritoryHasNoRapporteurException(string message) : base(message)
        {
        }
    }
}
