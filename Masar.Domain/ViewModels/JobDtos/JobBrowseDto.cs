using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.JobDtos
{
    public class JobBrowseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogo { get; set; }
        public string Location { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public string WorkMode { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string PostedDateDisplay { get; set; } = string.Empty;
        public string? SalaryDisplay { get; set; }
        public string DescriptionSnippet { get; set; } = string.Empty;
        public bool IsSaved { get; set; }
    }
}
