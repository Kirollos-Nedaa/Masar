using Masar.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CompanyDtos
{
    public class CompanyProfileDto
    {
        // ── Logo ──────────────────────────────────────────────
        public string? LogoUrl { get; set; }

        // ── Basic Information ─────────────────────────────────
        public string? CompanyName { get; set; }
        public string? Industry { get; set; }
        public CompanySize? Size { get; set; }
        public string? Description { get; set; }

        // ── Contact Information ───────────────────────────────
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }

        // ── Social Links ──────────────────────────────────────
        public List<ProfessionalLinkDto> ProfessionalLinks { get; set; } = new();
    }
}
