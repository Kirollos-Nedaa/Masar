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

            if (dateTime <= DateTime.Now)
            {
                var message = ErrorMessage ?? $"Date must be after {DateTime.Now.ToString("MM/dd/yyyy - HH:mm")}";
                return new ValidationResult(message);
            }

            return ValidationResult.Success;
        }
    }
}
