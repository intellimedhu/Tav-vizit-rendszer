using OrganiMedCore.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrganiMedCore.Core.Helpers
{
    public static class OrganizationUserHelper
    {
        public static string ConvertToTextOrganizationUserProfileType(OrganizationUserProfileTypes? organizationUserProfileType)
        {
            switch (organizationUserProfileType)
            {
                case OrganizationUserProfileTypes.Doctor:
                    return "Doktor";
                case OrganizationUserProfileTypes.SpecialAssistant:
                    return "Szakasszisztens";
                case OrganizationUserProfileTypes.Assistant:
                    return "Asszisztens";
                case OrganizationUserProfileTypes.Receptionist:
                    return "Recepciós";
                case OrganizationUserProfileTypes.Other:
                    return "Egyéb";
                default:
                    return "";
            }
        }
    }
}
