using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CandidateDtos
{
    public class EducationDto
    {
        [Required(ErrorMessage = "University is required.")]
        public string? University { get; set; }

        [Required(ErrorMessage = "Degree is required.")]
        public string? Degree { get; set; }

        [Required(ErrorMessage = "Major is required.")]
        public string? Major { get; set; }

        [Required(ErrorMessage = "Start year is required.")]
        public DateOnly? StartYear { get; set; }

        [Required(ErrorMessage = "Expected graduation is required.")]
        public DateOnly? ExpectedGraduation { get; set; }
    }
}
