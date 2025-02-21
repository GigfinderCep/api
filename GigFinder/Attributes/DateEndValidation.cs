using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.ComponentModel.DataAnnotations;

namespace GigFinder.Attributes
{
    public class DateEndValidation : ValidationAttribute
    {
        private readonly string _startDatePropertyName;

        public DateEndValidation(string startDatePropertyName)
        {
            _startDatePropertyName = startDatePropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var startDateProperty = validationContext.ObjectType.GetProperty(_startDatePropertyName);
            if (startDateProperty == null)
            {
                return new ValidationResult($"Unknown property: {_startDatePropertyName}");
            }

            var startDateValue = (DateTime)startDateProperty.GetValue(validationContext.ObjectInstance);
            var endDateValue = (DateTime)value;

            if (endDateValue < startDateValue)
            {
                return new ValidationResult("End date must be greater than or equal to start date.");
            }

            return ValidationResult.Success;
        }
    }

}