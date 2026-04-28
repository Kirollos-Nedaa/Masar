using Masar.Domain.Enums;
using Masar.Domain.ViewModels.CandidateDtos;
using Masar.Domain.ViewModels.CompanyDtos;
using Masar.Domain.ViewModels.JobDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Core.IService
{
    public interface IApplicationService
    {
        // ── Candidate: apply ──────────────────────────────────
        Task<ApplyJobViewDto?> GetApplyViewAsync(int jobId, string userId);
        Task<(bool Success, string? Error)> SubmitApplicationAsync(int jobId, string userId, ApplyJobDto dto, string? resumeUrl);

        // ── Candidate: track applications ─────────────────────
        Task<List<CandidateApplicationDto>> GetCandidateApplicationsAsync(string userId);

        // ── Candidate: saved jobs ─────────────────────────────
        Task<List<SavedJobDto>> GetSavedJobsAsync(string userId);
        Task<(bool Success, string? Error)> ToggleSaveJobAsync(int jobId, string userId);

        // ── Company: review applicants ────────────────────────
        Task<ApplicantsViewDto?> GetApplicantsAsync(int jobId, string companyUserId, string? searchQuery = null, string? statusFilter = null, string? sortFilter = null, int page = 1, int pageSize = 6);
        Task<ReviewApplicationViewDto?> StartReviewAsync(int applicationId, string companyUserId);
        Task<bool> AcceptApplicationAsync(int applicationId, string companyUserId);
        Task<bool> RejectApplicationAsync(int applicationId, string companyUserId);
        Task<(bool Success, string? Error)> UpdateApplicationStatusAsync(string userId, int applicationId, ApplicationStatus newStatus);
    }
}
