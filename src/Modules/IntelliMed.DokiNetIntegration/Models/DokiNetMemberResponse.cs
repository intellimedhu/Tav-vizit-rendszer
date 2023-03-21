using IntelliMed.DokiNetIntegration.Models;
using System.Collections.Generic;

namespace IntelliMed.DokiNetIntegration.Http
{
    public class DokiNetMemberResponse<T> where T : DokiNetMember
    {
        //public int ResponseStatus { get; set; }

        public IEnumerable<T> Members { get; set; } = new List<T>();
    }
}
