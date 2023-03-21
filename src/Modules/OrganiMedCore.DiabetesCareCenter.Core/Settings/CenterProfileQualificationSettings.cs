using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.Settings
{
    public class CenterProfileQualificationSettings
    {
        /// <summary>
        /// List of qualifications where names are unique in.
        /// </summary>
        public IList<Qualification> Qualifications { get; set; } = new List<Qualification>();

        /// <summary>
        /// Required qualifications per occupations.
        /// </summary>
        public IDictionary<Occupation, HashSet<Guid>> QualificationsPerOccupations { get; set; } = new Dictionary<Occupation, HashSet<Guid>>();

        /// <summary>
        /// Returns the required qualifications for the given occupation.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<Guid> this[Occupation occupation]
        {
            get => QualificationsPerOccupations.Where(x => x.Key == occupation).SelectMany(x => x.Value);
        }
    }
}
