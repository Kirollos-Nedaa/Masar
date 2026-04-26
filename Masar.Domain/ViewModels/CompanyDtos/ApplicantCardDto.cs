using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CompanyDtos
{
    public class ApplicantCardDto
    {
        public int ApplicationId { get; set; }
        public int CandidateProfileId { get; set; }

        // Identity
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }

        // Application meta
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }

        // Profile data shown on the card
        public List<string> Skills { get; set; } = [];
        public string? LatestEducation { get; set; }

        // Links
        public List<ProfessionalLinkDto> professionalLinks { get; set; } = [];
        public string? ResumeUrl { get; set; }
    }
}
