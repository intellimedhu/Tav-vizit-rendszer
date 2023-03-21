using IntelliMed.Core;
using IntelliMed.Core.Constants;
using IntelliMed.Core.Extensions;
using IntelliMed.Core.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrganiMedCore.Core.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.Core.ViewModels
{
    public class EVisitPatientProfilePartViewModel : IContentPartViewModel<EVisitPatientProfilePart>
    {
        public string Title { get; set; }

        [Required(ErrorMessage = "A keresztnév megadása kötelező.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "A vezetéknév megadása kötelező.")]
        public string LastName { get; set; }

        public string Suffix { get; set; }

        public string BirthName { get; set; }

        public PatientIdentifierViewModel PatientIdentifierViewModel { get; set; }

        [RegularExpression(RegexPatterns.Email, ErrorMessage = "Az e-mail cím formátuma nem megfelelő.")]
        public string Email { get; set; }

        public string MothersName { get; set; }

        [Required(ErrorMessage = "A születési dátum megadása kötelező.")]
        public DateTime? DateOfBirth { get; set; }

        public string PlaceOfBirth { get; set; }

        public string Address { get; set; }

        public string Nationality { get; set; }

        public Sex Sex { get; set; }

        public bool CreateSharedUser { get; set; }

        [BindNever]
        public string FullName => $"{Title} {LastName} {FirstName} {Suffix}";


        public void UpdateViewModel(EVisitPatientProfilePart part)
        {
            part.ThrowIfNull();

            Title = part.Title;
            FirstName = part.FirstName;
            LastName = part.LastName;
            Suffix = part.Suffix;
            BirthName = part.BirthName;
            Email = part.Email;
            MothersName = part.MothersName;
            DateOfBirth = part.DateOfBirth;
            PlaceOfBirth = part.PlaceOfBirth;
            Address = part.Address;
            Nationality = part.Nationality;
            Sex = part.Sex;
            PatientIdentifierViewModel = new PatientIdentifierViewModel
            {
                Type = part.PatientIdentifierType,
                Value = part.PatientIdentifierValue
            };
            CreateSharedUser = part.CreateSharedUser;
        }

        public void UpdatePart(EVisitPatientProfilePart part)
        {
            part.ThrowIfNull();

            part.Title = Title;
            part.FirstName = FirstName;
            part.LastName = LastName;
            part.Suffix = Suffix;
            part.BirthName = BirthName;
            part.Email = Email;
            part.MothersName = MothersName;
            part.DateOfBirth = DateOfBirth;
            part.PlaceOfBirth = PlaceOfBirth;
            part.Address = Address;
            part.Nationality = Nationality;
            part.Sex = Sex;
            part.PatientIdentifierType = PatientIdentifierViewModel.Type;
            part.PatientIdentifierValue = PatientIdentifierViewModel.Value;
            part.CreateSharedUser = CreateSharedUser;
        }
    }
}
