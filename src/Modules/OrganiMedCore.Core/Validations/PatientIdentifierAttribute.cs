using IntelliMed.Core.Constants;
using OrganiMedCore.Core.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace OrganiMedCore.Core.Validations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PatientIdentifierAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;


        public PatientIdentifierAttribute(string otherProperty)
        {
            _otherProperty = otherProperty;
        }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_otherProperty);
            if (property == null)
            {
                return new ValidationResult("Unknown property: " + _otherProperty);
            }

            var otherPropertyValue = property.GetValue(validationContext.ObjectInstance);
            if (otherPropertyValue == null || !(otherPropertyValue is PatientIdentifierTypes))
            {
                return new ValidationResult("Invalid type of property: " + _otherProperty);
            }

            var identifierType = (PatientIdentifierTypes)otherPropertyValue;
            var valueString = value == null ? "" : value.ToString();
            switch (identifierType)
            {
                case PatientIdentifierTypes.Taj:
                    if (!Regex.IsMatch(valueString, RegexPatterns.Taj)) return Error();
                    break;

                case PatientIdentifierTypes.PassportNumber:
                case PatientIdentifierTypes.SeekingAsylumIdNumber:
                case PatientIdentifierTypes.EuropeanHealthInsuranceCard:
                    if (string.IsNullOrEmpty(valueString)) return Error();
                    break;
            }

            return ValidationResult.Success;
        }

        private ValidationResult Error() => new ValidationResult("A megadott azonosító nem megfelelő formátumú.");
    }
}
