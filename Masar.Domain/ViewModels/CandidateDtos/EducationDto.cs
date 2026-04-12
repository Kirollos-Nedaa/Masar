using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CandidateDtos
{
    public class EducationDto
    {
        public string? University { get; set; }
        public string? Degree { get; set; }
        public string? Major { get; set; }
        public DateOnly? StartYear { get; set; }
        public DateOnly? ExpectedGraduation { get; set; }
    }
}
