using System.ComponentModel.DataAnnotations;

namespace Masar.Domain.ViewModels.CompanyDtos
{
    public class EditCompanyContactDto
    {
        [Display(Name = "Office Address")]
        public string? Address { get; set; }

        [Display(Name = "Contact Phone")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Required(ErrorMessage = "Please Fill the Phone Number!")]
        public string ContactPhone { get; set; }

        [Display(Name = "Contact Email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Required(ErrorMessage = "Please Fill the Email Address!")]
        public string ContactEmail { get; set; }
    }
}