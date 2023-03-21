using IntelliMed.Core;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrganiMedCore.Core.Models.Enums;
using System;
using System.Collections.Generic;

namespace OrganiMedCore.Core.Models
{
    public class EVisitPatientProfilePart : ContentPart
    {
        public string Title { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Suffix { get; set; }

        public string BirthName { get; set; }

        public string Email { get; set; }

        public string MothersName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string PlaceOfBirth { get; set; }

        public string Address { get; set; }

        public string Nationality { get; set; }

        public Sex Sex { get; set; }

        public string PatientIdentifierValue { get; set; }

        public PatientIdentifierTypes PatientIdentifierType { get; set; }

        public int SharedUserId { get; set; }

        public bool CreateSharedUser { get; set; }

        public HashSet<string> AttendedOrganizationNames { get; set; } = new HashSet<string>();

        [BindNever]
        public string FullName => $"{Title} {LastName} {FirstName} {Suffix}";
    }
}
