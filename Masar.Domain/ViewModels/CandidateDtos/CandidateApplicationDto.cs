using Masar.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CandidateDtos
{
    public class CandidateApplicationDto
    {
        public int ApplicationId { get; set; }
        public int JobId { get; set; }

        // ── Job info ──────────────────────────────────────────
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogo { get; set; }
        public string Location { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public string? SalaryDisplay { get; set; }

        // ── Application state ─────────────────────────────────
        public ApplicationStatus Status { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;
        public string AppliedDateDisplay { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }
    }
}
