namespace OrganiMedCore.DiabetesCareCenter.Core.Exceptions
{
    public class SettlementHasNoTerritoryException : TerritoryException
    {
        public SettlementHasNoTerritoryException()
        {
        }

        public SettlementHasNoTerritoryException(string message) : base(message)
        {
        }
    }
}
