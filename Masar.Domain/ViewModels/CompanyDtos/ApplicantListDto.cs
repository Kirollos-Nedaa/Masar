using Masar.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CompanyDtos
{
    public class ApplicantListDto
    {
        public int ApplicationId { get; set; }
        public int CandidateProfileId { get; set; }

        // ── Candidate info ────────────────────────────────────
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }
        public string? ResumeUrl { get; set; }
        public string? CoverLetterUrl { get; set; }

        // ── Application info ──────────────────────────────────
        public ApplicationStatus Status { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;
        public string AppliedDateDisplay { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }

        // ── Skills snapshot ───────────────────────────────────
        public List<string> Skills { get; set; } = new();
    }
}
