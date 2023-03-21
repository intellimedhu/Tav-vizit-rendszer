using YesSql.Indexes;

namespace IntelliMed.DokiNetIntegration.Indexes
{
    public class DokiNetMemberIndex : MapIndex
    {
        public int? MemberRightId { get; set; }

        public string Prefix { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public string StampNumber { get; set; }
    }
}
