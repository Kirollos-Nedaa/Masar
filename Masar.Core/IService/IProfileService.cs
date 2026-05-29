using Masar.Domain.ViewModels;
using Masar.Domain.ViewModels.CandidateDtos;
using Masar.Domain.ViewModels.CompanyDtos;
using Microsoft.AspNetCore.Http;

namespace Masar.Core.IService
{
    public interface IProfileService
    {
        // ── Own profile ──────────────────────────────────────────
        Task<CandidateProfileDto> GetMyCandidateProfileAsync(string userId);
        Task<CompanyProfileDto> GetMyCompanyProfileAsync(string userId);

        // ── Other role's profile (read-only) ─────────────────────
        Task<CandidateProfileDto> GetCandidateProfileAsync(int candidateProfileId);
        Task<CompanyProfileDto> GetCompanyProfileAsync(int companyProfileId);

        // ── EDIT — Candidate ─────────────────────────────────────
        Task UpdatePersonalInfoAsync(string userId, PersonalInfoDto dto);
        Task UpdateEducationAsync(string userId, EducationDto dto);
        Task<EditSkillsDto> GetEditSkillsAsync(string userId);
        Task<(bool Success, string? ErrorMessage)> UpdateSkillsAsync(string userId, EditSkillsDto dto);
        Task UpdateCandidateLinksAsync(string userId, List<ProfessionalLinkDto> links);
        Task<(bool Success, string? Error)> UpdateResumeAsync(string userId, IFormFile file);
        Task<(bool Success, string? Error)> DeleteResumeAsync(string userId);
        Task<(bool Success, string? Error)> UpdateAvatarAsync(string userId, IFormFile file);

        // ── Candidate - saved jobs ─────────────────────────────
        Task<List<SavedJobDto>> GetSavedJobsAsync(string userId, string? search = null, string? sortBy = null);
        Task<(bool Success, string? Error)> ToggleSaveJobAsync(int jobId, string userId);
        Task<(bool Success, string? Error)> ClearSavedJobsAsync(string userId);

        // ── EDIT — Company ────────────────────────────────────────
        Task<EditCompanyInfoDto> GetCompanyInfoForEditAsync(string userId);
        Task<bool> UpdateCompanyInfoAsync(string userId, EditCompanyInfoDto dto);
        Task<EditCompanyContactDto> GetCompanyContactForEditAsync(string userId);
        Task<bool> UpdateCompanyContactAsync(string userId, EditCompanyContactDto dto);

        Task UpdateCompanyLinksAsync(string userId, List<ProfessionalLinkDto> links);
        Task<(bool Success, string? Error)> UpdateLogoAsync(string userId, IFormFile file);
        Task<(bool Success, string? Error)> DeleteLogoAsync(string userId);
    }
}