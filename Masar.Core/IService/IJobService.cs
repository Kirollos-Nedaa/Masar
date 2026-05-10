using Masar.Domain.ViewModels;
using Masar.Domain.ViewModels.CompanyDtos;
using Masar.Domain.ViewModels.Job;
using Masar.Domain.ViewModels.JobDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Core.IService
{
    public interface IJobService
    {
        // ── Company: manage jobs ──────────────────────────────
        Task<int> PostJobAsync(string userId, PostJobDto dto);
        Task<bool> UpdateJobAsync(string userId, int jobId, PostJobDto dto);
        Task<bool> DeleteJobAsync(string userId, int jobId);
        Task<bool> ToggleJobStatusAsync(string userId, int jobId);
        Task<PostJobDto?> GetJobForEditAsync(string userId, int jobId);
        Task<CompanyJobsViewDto> GetCompanyJobsAsync(string userId, int page = 1, int pageSize = 10);

        // ── Candidate: browse jobs ────────────────────────────
        Task<JobBrowseResultDto> BrowseJobsAsync(JobFilterDto filter, string? candidateUserId = null);
        Task<JobDetailDto?> GetJobDetailAsync(int jobId, string? candidateUserId = null);
    }
}
