using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.JobDtos
{
    public class JobDetailDto
    {
        public int Id { get; set; }

        // ── Job Info ──────────────────────────────────────────
        public string Title { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public string WorkMode { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string? Benefits { get; set; }
        public string PostedDateDisplay { get; set; } = string.Empty;
        public string? SalaryDisplay { get; set; }
        public int ApplicantCount { get; set; }
        public int NumberOfOpenings { get; set; }
        public DateTime ApplicationDeadline { get; set; }
        public bool RequireCv { get; set; }
        public bool RequireCoverLetter { get; set; }
        public bool IsActive { get; set; }

        // ── Company Info ──────────────────────────────────────
        public int CompanyProfileId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogo { get; set; }
        public string? CompanyDescription { get; set; }
        public string? CompanyIndustry { get; set; }
        public string? CompanySize { get; set; }
        public string? CompanyLocation { get; set; }

        // ── Candidate State ───────────────────────────────────
        public bool IsSaved { get; set; }
        public bool HasApplied { get; set; }
    }
}
