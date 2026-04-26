using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CompanyDtos
{
    public class ApplicantsViewDto
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public int NumberOfOpenings { get; set; }
        public List<ApplicantCardDto> Applicants { get; set; } = [];

        // Status counts (for the stat cards at the top)
        public int TotalApplicants { get; set; }
        public int AcceptedCount { get; set; }
        public int UnderReviewCount { get; set; }
        public int RejectedCount { get; set; }

        // Filter state
        public string? SearchQuery { get; set; }
        public string? StatusFilter { get; set; }
        public string? SortFilter { get; set; }
    }
}
