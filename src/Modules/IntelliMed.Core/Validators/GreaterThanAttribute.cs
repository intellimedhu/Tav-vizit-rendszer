using System;
using System.ComponentModel.DataAnnotations;

namespace IntelliMed.Core.Validators
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GreaterThanAttribute : ValidationAttribute
    {
        private readonly string _otherPropertyName;


        public GreaterThanAttribute(string otherPropertyName)
        {
            _otherPropertyName = otherPropertyName;
        }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyInfo = validationContext.ObjectType.GetProperty(_otherPropertyName);
            var propertyValue = propertyInfo.GetValue(validationContext.ObjectInstance, null);

            if (value is DateTime && propertyValue is DateTime)
            {
                var date1 = (DateTime)value;
                var date2 = (DateTime)propertyValue;

                return date1 > date2 ? ValidationResult.Success : new ValidationResult(ErrorMessage);
            }

            if (value is TimeSpan && propertyValue is TimeSpan)
            {
                var time1 = (TimeSpan)value;
                var time2 = (TimeSpan)propertyValue;

                return time1 > time2 ? ValidationResult.Success : new ValidationResult(ErrorMessage);
            }

            throw new ArgumentException("Invalid attribute usage. Allowed to compare DateTime and TimeSpan properties only.");
        }
    }
}
