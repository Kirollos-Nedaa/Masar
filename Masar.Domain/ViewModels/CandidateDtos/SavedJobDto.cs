using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CandidateDtos
{
    public class SavedJobDto
    {
        public int SavedJobId { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogo { get; set; }
        public string Location { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public string? SalaryDisplay { get; set; }
        public string PostedDateDisplay { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
