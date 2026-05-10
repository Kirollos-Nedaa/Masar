using Masar.Domain.Enums;
using Masar.Domain.Validation;
using System.ComponentModel.DataAnnotations;

namespace Masar.Domain.ViewModels.CompanyDtos
{
    public class CompanyInfoDto
    {
        [Required(ErrorMessage = "Company name is required.")]
        public string? CompanyName { get; set; }

        public string? Industry { get; set; }
        public CompanySize? Size { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(2000)]
        public string? Description { get; set; }

        public string? ContactEmail { get; set; }

        [Required(ErrorMessage = "Contact phone is required.")]
        [MinimumPhoneDigits(11, ErrorMessage = "Contact phone must contain at least 11 digits.")]
        public string? ContactPhone { get; set; }

        public string? Address { get; set; }
        public string? LogoUrl { get; set; }
    }
}
