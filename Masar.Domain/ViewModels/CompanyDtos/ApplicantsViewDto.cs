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

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 6;
        public int TotalCount { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }
}
