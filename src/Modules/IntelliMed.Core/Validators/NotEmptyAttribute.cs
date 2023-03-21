using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace IntelliMed.Core.Validators
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NotEmptyAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (!(value is IEnumerable list) || !list.GetEnumerator().MoveNext())
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
