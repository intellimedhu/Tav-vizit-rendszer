using System;

namespace OrganiMedCore.DiabetesCareCenter.Core.Models
{
    public class CenterProfileReviewState
    {
        public bool Accepted { get; set; }

        public string Post { get; set; }

        public DateTime Date { get; set; }

        public string Comment { get; set; }

        public bool Current { get; set; }
    }
}
