using Masar.Core.IService;
using Masar.Domain.Models;
using Masar.Domain.ViewModels;
using Masar.Domain.ViewModels.CandidateDtos;
using Masar.Domain.ViewModels.CompanyDtos;
using Masar.Infrastructure;
using Masar.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.Core.Services
{
    public class ProfileService : IProfileService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

            return MapToCandidateProfileDto(user, profile);
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

        public async Task<CompanyProfileDto> GetMyCompanyProfileAsync(string userId)
        {
            var profile = await _context.CompanyProfiles
                .Include(p => p.ContactInfo)
                .Include(p => p.ProfessionalLinks)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            return MapToCompanyProfileDto(profile);
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
            // Update ApplicationUser fields
            var user = await _userManager.FindByIdAsync(userId);
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.UserName = dto.Email; // keep username in sync with email
            await _userManager.UpdateAsync(user);

            // Update CandidateProfile fields
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

            // Get first education entry or create new one
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

        // -─ Add/remove single skill ───────────────────────────────

        public async Task<EditSkillsDto> GetEditSkillsAsync(string userId)
        {
            var profile = await _context.CandidateProfiles
                .Include(p => p.CandidateSkills)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var selectedIds = profile?.CandidateSkills
                .Select(cs => cs.SkillId)
                .ToHashSet() ?? new HashSet<int>();

            // ── Pull all skills into memory first, THEN filter ────
            var allSkills = await _context.Skills
                .OrderBy(s => s.Name)
                .Select(s => new SkillItemDto
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToListAsync(); // materialize first

            // Now set IsSelected in memory, not in EF query
            foreach (var skill in allSkills)
                skill.IsSelected = selectedIds.Contains(skill.Id);

            return new EditSkillsDto
            {
                CurrentSkills = allSkills.Where(s => s.IsSelected).ToList(),
                AvailableSkills = allSkills.Where(s => !s.IsSelected).ToList(),
                SelectedSkillIds = selectedIds.ToList()
            };
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateSkillsAsync(
            string userId, EditSkillsDto dto)
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

            // Handle custom skill if provided
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

            // Replace all candidate skills
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

        // ── Update links ────────────────────────────────────────
        public async Task UpdateCandidateLinksAsync(string userId, List<ProfessionalLinkDto> links)
        {
            var profile = await _context.CandidateProfiles
                .Include(p => p.ProfessionalLinks)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var candidateLinks = profile.ProfessionalLinks.ToList();
            _context.ProfessionalLinks.RemoveRange(candidateLinks);

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

            // ── Basic info ────────────────────────────────────────
            profile.Name = dto.CompanyName;
            profile.Industry = dto.Industry;
            profile.Size = dto.Size;
            profile.Description = dto.Description ?? string.Empty; // Ensure Description is not null
            profile.LogoUrl = dto.LogoUrl;

            // ── Contact info: upsert ──────────────────────────────
            if (profile.ContactInfo == null)
            {
                profile.ContactInfo = new CompanyContactInfo
                {
                    CompanyProfileId = profile.Id
                };
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

            // Ensure list is not null
            profile.ProfessionalLinks ??= new List<ProfessionalLink>();

            // Remove existing links
            _context.ProfessionalLinks.RemoveRange(profile.ProfessionalLinks);

            // Add new ones (handle null input too)
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

        // ── Mapping helpers ───────────────────────────────────────

        private static CandidateProfileDto MapToCandidateProfileDto(
            ApplicationUser? user, CandidateProfile? profile)
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
            dto.ResumeUrl = profile.ResumeUrl;

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