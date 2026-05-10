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

            if (dateTime <= DateTime.UtcNow)
            {
                var message = ErrorMessage ?? $"{validationContext.DisplayName} must be later than the current time.";
                return new ValidationResult(message);
            }

            return ValidationResult.Success;
        }
    }
}
