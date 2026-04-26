using Masar.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }
        public string? LogoUrl { get; set; }
    }
}
