using OrganiMedCore.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrganiMedCore.Core.Constants
{
    public static class LuceneEntryNames
    {
        public const string ContentType = "Content.ContentItem.ContentType";
        public static string FirstName = $"{nameof(EVisitPatientProfilePart)}.{nameof(EVisitPatientProfilePart.FirstName)}";
        public static string LastName = $"{nameof(EVisitPatientProfilePart)}.{nameof(EVisitPatientProfilePart.LastName)}";
        public static string MothersName = $"{nameof(EVisitPatientProfilePart)}.{nameof(EVisitPatientProfilePart.MothersName)}";
        public static string DateOfBirth = $"{nameof(EVisitPatientProfilePart)}.{nameof(EVisitPatientProfilePart.DateOfBirth)}";
        public static string Email = $"{nameof(EVisitPatientProfilePart)}.{nameof(EVisitPatientProfilePart.Email)}";
        public static string PatientIdentifierValue = $"{nameof(EVisitPatientProfilePart)}.{nameof(EVisitPatientProfilePart.PatientIdentifierValue)}";
        public static string AttendedOrganizationNames = $"{nameof(EVisitPatientProfilePart)}.{nameof(EVisitPatientProfilePart.AttendedOrganizationNames)}";

    }
}
