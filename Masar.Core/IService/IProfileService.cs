using Masar.Domain.Models;
using Masar.Domain.ViewModels;
using Masar.Domain.ViewModels.CandidateDtos;
using Masar.Domain.ViewModels.CompanyDtos;
using System.Threading.Tasks;

namespace Masar.Core.IService
{
    public interface IProfileService
    {
        // Own profile
        Task<CandidateProfileDto> GetMyCandidateProfileAsync(string userId);
        Task<CompanyProfileDto> GetMyCompanyProfileAsync(string userId);

        // Other role's profile (read-only, no edit data needed)
        Task<CandidateProfileDto> GetCandidateProfileAsync(int candidateProfileId);
        Task<CompanyProfileDto> GetCompanyProfileAsync(int companyProfileId);

        // ── EDIT — Candidate ─────────────────────────────────────
        Task UpdatePersonalInfoAsync(string userId, PersonalInfoDto dto);
        Task UpdateEducationAsync(string userId, EducationDto dto);
        Task<EditSkillsDto> GetEditSkillsAsync(string userId);
        Task<(bool Success, string? ErrorMessage)> UpdateSkillsAsync(string userId, EditSkillsDto dto);
        Task UpdateCandidateLinksAsync(string userId, List<ProfessionalLinkDto> links);

        // ── EDIT — Company ────────────────────────────────────────
        Task UpdateCompanyInfoAsync(string userId, CompanyInfoDto dto);
        Task UpdateCompanyLinksAsync(string userId, List<ProfessionalLinkDto> links);
    }
}