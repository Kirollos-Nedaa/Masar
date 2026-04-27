using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CompanyDtos
{
    public class ReviewApplicationViewDto
    {
        public int ApplicationId { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public int NumberOfOpenings { get; set; }

        // Candidate identity
        public int CandidateProfileId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }
        public string? Bio { get; set; }

        // Application meta
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }

        // Profile sections
        public List<string> Skills { get; set; } = [];
        public List<ReviewEducationDto> Educations { get; set; } = [];

        // Links / files
        public string? ResumeUrl { get; set; }
        public string? CoverLetter { get; set; }
        public List<ProfessionalLinkDto> professionalLinks { get; set; } = [];

        // Form answers (only present when the job had questions)
        public List<ReviewAnswerDto> Answers { get; set; } = [];
    }
}
