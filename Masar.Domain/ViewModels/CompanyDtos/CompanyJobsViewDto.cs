using Masar.Domain.ViewModels.Job;

namespace Masar.Domain.ViewModels.CompanyDtos
{
    public class CompanyJobsViewDto
    {
        public List<JobListItemDto> Jobs { get; set; } = [];
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }
}
