using System;
using System.Collections.Generic;
using System.Text;

namespace OrganiMedCore.Manager.ViewModels
{
    public class EditEVisitUserViewModel
    {
        public int Id { get; set; }

        public bool IsEVisitUser { get; set; }

        public bool EVisitLoginEnabled { get; set; }

        public PermittedTenantViewModel[] Tenants { get; set; } = new PermittedTenantViewModel[0];
    }
}
