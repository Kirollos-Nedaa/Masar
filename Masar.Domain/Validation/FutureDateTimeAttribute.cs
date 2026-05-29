using Masar.Domain.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Masar.Domain.Validation
{
    public class FutureDateTimeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime dateTime)
            {
                return ValidationResult.Success;
            }

            if (dateTime <= AppTime.Now)
            {
                var message = ErrorMessage ?? $"Date must be after {AppTime.Now.ToString("MM/dd/yyyy - hh:mmtt")}";
                return new ValidationResult(message);
            }

            return ValidationResult.Success;
        }
    }
}
