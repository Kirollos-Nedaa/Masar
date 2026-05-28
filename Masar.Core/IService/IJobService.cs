using Masar.Domain.ViewModels.CompanyDtos;
using Masar.Domain.ViewModels.Job;
using Masar.Domain.ViewModels.JobDtos;

namespace Masar.Core.IService
{
    public interface IJobService
    {
        // ── Company: manage jobs ──────────────────────────────
        Task<int> PostJobAsync(string userId, PostJobDto dto);
        Task<bool> UpdateJobAsync(string userId, int jobId, PostJobDto dto);
        Task<bool> DeleteJobAsync(string userId, int jobId);
        Task<(bool Success, bool WasExtended)> ToggleJobStatusAsync(string userId, int jobId);
        Task<PostJobDto?> GetJobForEditAsync(string userId, int jobId);
        Task<CompanyJobsViewDto> GetCompanyJobsAsync(string userId, int page = 1, int pageSize = 10);

        // ── Candidate: browse jobs ────────────────────────────
        Task<JobBrowseResultDto> BrowseJobsAsync(JobFilterDto filter, string? candidateUserId = null);
        Task<JobDetailDto?> GetJobDetailAsync(int jobId, string? candidateUserId = null);
    }
}
