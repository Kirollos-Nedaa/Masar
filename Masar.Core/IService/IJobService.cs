using Masar.Domain.ViewModels;
using Masar.Domain.ViewModels.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Core.IService
{
    public interface IJobService
    {
        Task<int> PostJobAsync(string userId, PostJobDto dto);
        Task<bool> UpdateJobAsync(string userId, int jobId, PostJobDto dto);
        Task<bool> DeleteJobAsync(string userId, int jobId);
        Task<bool> ToggleJobStatusAsync(string userId, int jobId);
        Task<PostJobDto?> GetJobForEditAsync(string userId, int jobId);
        Task<List<JobListItemDto>> GetCompanyJobsAsync(string userId);
    }
}