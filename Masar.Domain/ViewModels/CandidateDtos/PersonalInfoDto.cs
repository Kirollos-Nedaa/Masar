using Masar.Domain.Enums;
using Masar.Domain.Validation;
using System.ComponentModel.DataAnnotations;

namespace Masar.Domain.ViewModels.CandidateDtos
{
    public class PersonalInfoDto
    {
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [MinimumPhoneDigits(11, ErrorMessage = "Phone number must contain at least 11 digits.")]
        public string? PhoneNumber { get; set; }

        public Gender? Gender { get; set; }
        public string? Location { get; set; }
        public DateOnly? DateOfBirth { get; set; }

        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
        public string? Bio { get; set; }
    }
}
