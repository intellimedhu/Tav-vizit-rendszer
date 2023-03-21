using System;
using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenterManager.Settings
{
    public class DccmLoginNonce
    {
        public Dictionary<Guid, int> Storage { get; set; } = new Dictionary<Guid, int>();
    }
}
