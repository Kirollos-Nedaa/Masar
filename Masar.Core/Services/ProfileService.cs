using Masar.Core.IService;
using Masar.Domain.Enums;
using Masar.Domain.Helpers;
using Masar.Domain.Models;
using Masar.Domain.ViewModels;
using Masar.Domain.ViewModels.CandidateDtos;
using Masar.Domain.ViewModels.CompanyDtos;
using Masar.Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Masar.Core.Services
{
    public class ProfileService : IProfileService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;
        private readonly IJobLifecycleService _jobLifecycleService;

        public ProfileService(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IFileService fileService,
            IJobLifecycleService jobLifecycleService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _jobLifecycleService = jobLifecycleService;
        }

        // ── GET ───────────────────────────────────────────────────

        public async Task<CandidateProfileDto> GetMyCandidateProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var profile = await _context.CandidateProfiles
                .Include(p => p.Educations)
                .Include(p => p.CandidateSkills).ThenInclude(cs => cs.Skill)
                .Include(p => p.ProfessionalLinks)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var dto = MapToCandidateProfileDto(user, profile);

            if (user != null)
            {
                dto.LoginProviders = (await _userManager.GetLoginsAsync(user))
                                          .Select(l => l.LoginProvider)
                                          .ToList();
                dto.HasLocalPassword = await _userManager.HasPasswordAsync(user);
            }

            return dto;
        }

        public async Task<CompanyProfileDto> GetMyCompanyProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var profile = await _context.CompanyProfiles
                .Include(p => p.ContactInfo)
                .Include(p => p.ProfessionalLinks)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var dto = MapToCompanyProfileDto(profile);

            if (user != null)
            {
                dto.LoginProviders = (await _userManager.GetLoginsAsync(user))
                                          .Select(l => l.LoginProvider)
                                          .ToList();
                dto.HasLocalPassword = await _userManager.HasPasswordAsync(user);
            }

            return dto;
        }

        public async Task<CandidateProfileDto> GetCandidateProfileAsync(int candidateProfileId)
        {
            var profile = await _context.CandidateProfiles
                .Include(p => p.User)
                .Include(p => p.Educations)
                .Include(p => p.CandidateSkills).ThenInclude(cs => cs.Skill)
                .Include(p => p.ProfessionalLinks)
                .FirstOrDefaultAsync(p => p.Id == candidateProfileId);

            return MapToCandidateProfileDto(profile?.User, profile);
        }

        public async Task<CompanyProfileDto> GetCompanyProfileAsync(int companyProfileId)
        {
            var profile = await _context.CompanyProfiles
                .Include(p => p.ContactInfo)
                .Include(p => p.ProfessionalLinks)
                .FirstOrDefaultAsync(p => p.Id == companyProfileId);

            return MapToCompanyProfileDto(profile);
        }

        // ── EDIT — Candidate ──────────────────────────────────────

        public async Task UpdatePersonalInfoAsync(string userId, PersonalInfoDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.UserName = dto.Email;
            await _userManager.UpdateAsync(user);

            var profile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new CandidateProfile { UserId = userId };
                _context.CandidateProfiles.Add(profile);
            }

            profile.PhoneNumber = dto.PhoneNumber;
            profile.Gender = dto.Gender;
            profile.Location = dto.Location;
            profile.DateOfBirth = dto.DateOfBirth;
            profile.Bio = dto.Bio;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateEducationAsync(string userId, EducationDto dto)
        {
            var profile = await _context.CandidateProfiles
                .Include(p => p.Educations)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var education = profile.Educations.FirstOrDefault();

            if (education == null)
            {
                education = new Education { CandidateProfileId = profile.Id };
                _context.Educations.Add(education);
            }

            education.University = dto.University;
            education.Degree = dto.Degree;
            education.Major = dto.Major;
            education.StartYear = dto.StartYear ?? new DateOnly();
            education.ExpectedGraduation = dto.ExpectedGraduation ?? new DateOnly();

            await _context.SaveChangesAsync();
        }

        public async Task<EditSkillsDto> GetEditSkillsAsync(string userId)
        {
            var profile = await _context.CandidateProfiles
                .Include(p => p.CandidateSkills)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var selectedIds = profile?.CandidateSkills
                .Select(cs => cs.SkillId)
                .ToHashSet() ?? new HashSet<int>();

            var allSkills = await _context.Skills
                .OrderBy(s => s.Name)
                .Select(s => new SkillItemDto { Id = s.Id, Name = s.Name })
                .ToListAsync();

            foreach (var skill in allSkills)
                skill.IsSelected = selectedIds.Contains(skill.Id);

            return new EditSkillsDto
            {
                CurrentSkills = allSkills.Where(s => s.IsSelected).ToList(),
                AvailableSkills = allSkills.Where(s => !s.IsSelected).ToList(),
                SelectedSkillIds = selectedIds.ToList()
            };
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateSkillsAsync(string userId, EditSkillsDto dto)
        {
            if (dto.SelectedSkillIds.Count > EditSkillsDto.MaxSkills)
                return (false, $"You can have a maximum of {EditSkillsDto.MaxSkills} skills.");

            var profile = await _context.CandidateProfiles
                .Include(p => p.CandidateSkills)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new CandidateProfile { UserId = userId };
                _context.CandidateProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrWhiteSpace(dto.CustomSkillName))
            {
                var normalized = dto.CustomSkillName.Trim().ToLower();
                var existing = await _context.Skills
                    .FirstOrDefaultAsync(s => s.NormalizedName == normalized);

                if (existing == null)
                {
                    existing = new Skill
                    {
                        Name = dto.CustomSkillName.Trim(),
                        NormalizedName = normalized
                    };
                    _context.Skills.Add(existing);
                    await _context.SaveChangesAsync();
                }

                if (!dto.SelectedSkillIds.Contains(existing.Id))
                    dto.SelectedSkillIds.Add(existing.Id);
            }

            _context.CandidateSkills.RemoveRange(profile.CandidateSkills);
            foreach (var skillId in dto.SelectedSkillIds.Distinct())
            {
                _context.CandidateSkills.Add(new CandidateSkill
                {
                    CandidateProfileId = profile.Id,
                    SkillId = skillId
                });
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task UpdateCandidateLinksAsync(string userId, List<ProfessionalLinkDto> links)
        {
            var profile = await _context.CandidateProfiles
                .Include(p => p.ProfessionalLinks)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            _context.ProfessionalLinks.RemoveRange(profile.ProfessionalLinks.ToList());

            foreach (var link in links)
            {
                _context.ProfessionalLinks.Add(new ProfessionalLink
                {
                    CandidateProfileId = profile.Id,
                    LinksNames = link.LinkName,
                    Url = link.Url
                });
            }

            await _context.SaveChangesAsync();
        }

        // ── FILE UPLOADS — Candidate ──────────────────────────────
        public async Task<(bool Success, string? Error)> UpdateResumeAsync(string userId, IFormFile file)
        {
            try
            {
                var profile = await GetOrCreateCandidateProfileAsync(userId);

                if (!string.IsNullOrEmpty(profile.ResumeUrl))
                    _fileService.DeleteFile(profile.ResumeUrl);

                var url = await _fileService.SaveResumeAsync(file, userId);

                profile.ResumeUrl = url;
                profile.ResumeOriginalName = file.FileName;
                profile.ResumeUploadedAt = AppTime.Now.ToDetailedDisplayDate();

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (InvalidOperationException ex) { return (false, ex.Message); }
            catch (Exception ex) { return (false, "An unexpected error occurred while saving your resume."); }
        }

        public async Task<(bool Success, string? Error)> DeleteResumeAsync(string userId)
        {
            try
            {
                var profile = await _context.CandidateProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (profile == null) return (false, "Profile not found.");

                _fileService.DeleteFile(profile.ResumeUrl);
                profile.ResumeUrl = null;
                profile.ResumeOriginalName = null;

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch { return (false, "An unexpected error occurred while deleting your resume."); }
        }

        public async Task<(bool Success, string? Error)> UpdateAvatarAsync(string userId, IFormFile file)
        {
            try
            {
                var profile = await GetOrCreateCandidateProfileAsync(userId);

                // Delete old avatar if one exists
                _fileService.DeleteFile(profile.AvatarUrl);

                var url = await _fileService.SaveAvatarAsync(file, userId);

                profile.AvatarUrl = url;
                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message);
            }
            catch
            {
                return (false, "An unexpected error occurred while saving your profile picture.");
            }
        }

        // ── SAVED JOBS — Candidate ─────────────────────────────────
        public async Task<List<SavedJobDto>> GetSavedJobsAsync(string userId, string? search = null, string? sortBy = null)
        {
            await _jobLifecycleService.CloseExpiredJobsAsync();

            var profile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return new List<SavedJobDto>();

            var query = _context.SavedJobs
                .Where(s => s.CandidateProfileId == profile.Id)
                .AsQueryable();

            // ── Search ────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(sj =>
                    sj.Job.Title.ToLower().Contains(s) ||
                    sj.Job.Company.Name.ToLower().Contains(s) ||
                    sj.Job.Location.ToLower().Contains(s));
            }

            // ── Sort ──────────────────────────────────────────
            query = sortBy switch
            {
                "oldest" => query.OrderBy(sj => sj.SavedAt),
                "title" => query.OrderBy(sj => sj.Job.Title),
                _ => query.OrderByDescending(sj => sj.SavedAt)
            };

            return await query
                .Select(s => new SavedJobDto
                {
                    SavedJobId = s.Id,
                    JobId = s.JobId,
                    JobTitle = s.Job.Title,
                    CompanyName = s.Job.Company.Name,
                    CompanyLogo = s.Job.Company.LogoUrl,
                    Location = s.Job.Location,
                    JobType = s.Job.JobType.ToString(),
                    SalaryDisplay = s.Job.MinSalary.ToSalaryDisplay(s.Job.MaxSalary),
                    PostedDateDisplay = s.Job.PostedDate.ToRelativeDate(),
                    DescriptionSnippet = s.Job.Description.Length > 120
                                              ? s.Job.Description.Substring(0, 120) + "..."
                                              : s.Job.Description,
                    IsActive = s.Job.IsActive
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string? Error)> ClearSavedJobsAsync(string userId)
        {
            var profile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return (false, "Profile not found.");

            var savedJobs = await _context.SavedJobs
                .Where(s => s.CandidateProfileId == profile.Id)
                .ToListAsync();

            if (savedJobs.Any())
            {
                _context.SavedJobs.RemoveRange(savedJobs);
                await _context.SaveChangesAsync();
            }

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> ToggleSaveJobAsync(int jobId, string userId)
        {
            var profile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return (false, "Profile not found.");

            var existing = await _context.SavedJobs
                .FirstOrDefaultAsync(s => s.CandidateProfileId == profile.Id && s.JobId == jobId);

            if (existing != null)
            {
                _context.SavedJobs.Remove(existing);
                await _context.SaveChangesAsync();
                return (true, null);
            }

            _context.SavedJobs.Add(new SavedJob
            {
                CandidateProfileId = profile.Id,
                JobId = jobId,
                SavedAt = AppTime.Now
            });

            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ── EDIT — Company ────────────────────────────────────────

        public async Task UpdateCompanyInfoAsync(string userId, CompanyInfoDto dto)
        {
            var profile = await _context.CompanyProfiles
                .Include(p => p.ContactInfo)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new CompanyProfile { UserId = userId };
                _context.CompanyProfiles.Add(profile);
            }

            profile.Name = dto.CompanyName;
            profile.Industry = dto.Industry == Industries.None ? null : dto.Industry.ToString();
            profile.Size = dto.Size;
            profile.Description = dto.Description ?? string.Empty;

            if (profile.ContactInfo == null)
            {
                profile.ContactInfo = new CompanyContactInfo { CompanyProfileId = profile.Id };
                _context.CompanyContactInfos.Add(profile.ContactInfo);
            }

            profile.ContactInfo.Email = dto.ContactEmail;
            profile.ContactInfo.PhoneNumber = dto.ContactPhone;
            profile.ContactInfo.Address = dto.Address;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateCompanyLinksAsync(string userId, List<ProfessionalLinkDto> links)
        {
            var profile = await _context.CompanyProfiles
                .Include(p => p.ProfessionalLinks)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new CompanyProfile { UserId = userId };
                _context.CompanyProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            profile.ProfessionalLinks ??= new List<ProfessionalLink>();
            _context.ProfessionalLinks.RemoveRange(profile.ProfessionalLinks);

            if (links != null && links.Any())
            {
                foreach (var link in links)
                {
                    _context.ProfessionalLinks.Add(new ProfessionalLink
                    {
                        CompanyProfileId = profile.Id,
                        Url = link.Url,
                        LinksNames = link.LinkName
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        // ── FILE UPLOADS — Company ────────────────────────────────
        public async Task<(bool Success, string? Error)> UpdateLogoAsync(string userId, IFormFile file)
        {
            try
            {
                var profile = await _context.CompanyProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (profile == null)
                    return (false, "Company profile not found.");

                // Delete old logo if one exists
                _fileService.DeleteFile(profile.LogoUrl);

                var url = await _fileService.SaveLogoAsync(file, profile.Id.ToString());

                profile.LogoUrl = url;
                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message);
            }
            catch
            {
                return (false, "An unexpected error occurred while saving the logo.");
            }
        }

        public async Task<(bool Success, string? Error)> DeleteLogoAsync(string userId)
        {
            try
            {
                var profile = await _context.CompanyProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (profile == null)
                    return (false, "Company profile not found.");

                if (!string.IsNullOrEmpty(profile.LogoUrl))
                {
                    _fileService.DeleteFile(profile.LogoUrl);
                    profile.LogoUrl = null;
                    await _context.SaveChangesAsync();
                }

                return (true, null);
            }
            catch
            {
                return (false, "An unexpected error occurred while deleting the logo.");
            }
        }

        // ── Private helpers ───────────────────────────────────────

        private async Task<CandidateProfile> GetOrCreateCandidateProfileAsync(string userId)
        {
            var profile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new CandidateProfile { UserId = userId };
                _context.CandidateProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            return profile;
        }

        private static CandidateProfileDto MapToCandidateProfileDto(ApplicationUser? user, CandidateProfile? profile)
        {
            var dto = new CandidateProfileDto
            {
                FirstName = user?.FirstName,
                LastName = user?.LastName,
                Email = user?.Email,
            };

            if (profile == null) return dto;

            dto.PhoneNumber = profile.PhoneNumber;
            dto.Gender = profile.Gender;
            dto.Location = profile.Location;
            dto.DateOfBirth = profile.DateOfBirth;
            dto.Bio = profile.Bio;
            dto.ResumeOriginalName = profile.ResumeOriginalName;
            dto.ResumeUrl = profile.ResumeUrl;
            dto.ResumeUploadedAt = profile.ResumeUploadedAt;
            dto.AvatarUrl = profile.AvatarUrl;

            dto.Education = profile.Educations.FirstOrDefault() is { } edu
                ? new EducationDto
                {
                    University = edu.University,
                    Degree = edu.Degree,
                    Major = edu.Major,
                    StartYear = edu.StartYear,
                    ExpectedGraduation = edu.ExpectedGraduation
                }
                : null;

            dto.SkillIds = profile.CandidateSkills.Select(cs => cs.SkillId).ToList();
            dto.SkillNames = profile.CandidateSkills.Select(cs => cs.Skill.Name).ToList();

            dto.ProfessionalLinks = profile.ProfessionalLinks.Select(pl => new ProfessionalLinkDto
            {
                Id = pl.Id,
                LinkName = pl.LinksNames,
                Url = pl.Url
            }).ToList();

            return dto;
        }

        private static CompanyProfileDto MapToCompanyProfileDto(CompanyProfile? profile)
        {
            if (profile == null) return new CompanyProfileDto();

            return new CompanyProfileDto
            {
                LogoUrl = profile.LogoUrl,
                CompanyName = profile.Name,
                Industry = profile.Industry,
                Size = profile.Size,
                Description = profile.Description,
                ContactEmail = profile.ContactInfo?.Email,
                ContactPhone = profile.ContactInfo?.PhoneNumber,
                Address = profile.ContactInfo?.Address,
                ProfessionalLinks = profile.ProfessionalLinks.Select(pl => new ProfessionalLinkDto
                {
                    Id = pl.Id,
                    LinkName = pl.LinksNames,
                    Url = pl.Url
                }).ToList()
            };
        }
    }
}