using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CandidateDtos
{
    public class CandidateProfileSectionsDto
    {
        // ── Skills ────────────────────────────────────────────
        public List<int> SkillIds { get; set; } = new();

        // ── Resume ────────────────────────────────────────────
        public string? ResumeUrl { get; set; }

        // ── Professional Links ────────────────────────────────
        public List<ProfessionalLinkDto> ProfessionalLinks { get; set; } = new();
    }
}
