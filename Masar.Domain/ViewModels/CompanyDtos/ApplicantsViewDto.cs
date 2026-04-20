using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CompanyDtos
{
    public class ApplicantsViewDto
    {
        // ── Job context ───────────────────────────────────────
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string JobLocation { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // ── Stats ─────────────────────────────────────────────
        public int TotalApplicants { get; set; }
        public int UnderReview { get; set; }
        public int Accepted { get; set; }
        public int Rejected { get; set; }

        // ── Applicants list ───────────────────────────────────
        public List<ApplicantListDto> Applicants { get; set; } = new();

        // ── Active filter ─────────────────────────────────────
        public string? StatusFilter { get; set; }
    }
}
