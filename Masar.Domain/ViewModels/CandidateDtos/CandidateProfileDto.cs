using Masar.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CandidateDtos
{
    public class CandidateProfileDto
    {
        public string? AvatarUrl { get; set; }

        // ── Personal Information ──────────────────────────────
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public Gender? Gender { get; set; }
        public string? Location { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Bio { get; set; }

        // ── Education ─────────────────────────────────────────
        public EducationDto? Education { get; set; }

        // ── Skills ────────────────────────────────────────────
        public List<int>? SkillIds { get; set; } = new();
        public List<string>? SkillNames { get; set; } = new();

        // ── Resume ───────────────────────────────────────────
        public string? ResumeUrl { get; set; }
        public string? ResumeOriginalName { get; set; }
        public string? ResumeUploadedAt { get; set; }

        // ── Professional Links ────────────────────────────────
        public List<ProfessionalLinkDto>? ProfessionalLinks { get; set; } = new();

        public IList<string> LoginProviders { get; set; } = new List<string>();
        public bool HasLocalPassword { get; set; }
    }
}
