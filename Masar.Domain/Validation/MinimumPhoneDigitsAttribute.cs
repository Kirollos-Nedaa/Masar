using System.ComponentModel.DataAnnotations;

namespace Masar.Domain.Validation
{
    public class MinimumPhoneDigitsAttribute : ValidationAttribute
    {
        private readonly int _minimumDigits;

        public MinimumPhoneDigitsAttribute(int minimumDigits)
        {
            _minimumDigits = minimumDigits;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string phoneNumber || string.IsNullOrWhiteSpace(phoneNumber))
            {
                return ValidationResult.Success;
            }

            var digitCount = phoneNumber.Count(char.IsDigit);
            if (digitCount < _minimumDigits)
            {
                var message = ErrorMessage ?? $"{validationContext.DisplayName} must contain at least {_minimumDigits} digits.";
                return new ValidationResult(message);
            }

            return ValidationResult.Success;
        }
    }
}
