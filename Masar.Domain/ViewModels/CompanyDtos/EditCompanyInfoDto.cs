using System.ComponentModel.DataAnnotations;
using Masar.Domain.Enums;

namespace Masar.Domain.ViewModels.CompanyDtos
{
    public class EditCompanyInfoDto
    {
        [Required(ErrorMessage = "Company Name is required")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Industry is required")]
        [Display(Name = "Industry")]
        public Industries Industry { get; set; }

        [Required(ErrorMessage = "Company Size is required")]
        [Display(Name = "Company Size")]
        public CompanySize Size { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "About the Company")]
        public string Description { get; set; }
    }
}