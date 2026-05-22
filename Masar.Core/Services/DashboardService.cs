using Masar.Core.IService;
using Masar.Domain.Enums;
using Masar.Domain.Helpers;
using Masar.Domain.Models;
using Masar.Domain.ViewModels;
using Masar.Domain.ViewModels.AdminDtos;
using Masar.Domain.ViewModels.CandidateDtos;
using Masar.Domain.ViewModels.CompanyDtos;
using Masar.Domain.ViewModels.Job;
using Masar.Infrastructure.Constants;
using Masar.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Masar.Core.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly IProfileService _profileService;
        private readonly IJobLifecycleService _jobLifecycleService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DashboardService(
            AppDbContext context,
            IProfileService profileService,
            IJobLifecycleService jobLifecycleService,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _profileService = profileService;
            _jobLifecycleService = jobLifecycleService;
            _roleManager = roleManager;
        }


        //-───────────── CANDIDATE DASHBOARD -────────────────────────────────────────────
        public async Task<CandidateDashboardDto> GetCandidateDashboardAsync(string userId)
        {
            await _jobLifecycleService.CloseExpiredJobsAsync();

            var user = await _context.Users.FindAsync(userId);

            var profile = await _context.CandidateProfiles
                .Include(p => p.Educations)
                .Include(p => p.CandidateSkills)
                .Include(p => p.ProfessionalLinks)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // ── Stats ──────────────────────────────────────────
            var totalApps = profile == null ? 0 :
                await _context.JobApplications
                    .CountAsync(a => a.CandidateProfileId == profile.Id);

            var savedJobs = profile == null ? 0 :
                await _context.SavedJobs
                    .CountAsync(s => s.CandidateProfileId == profile.Id);

            var underReview = profile == null ? 0 :
                await _context.JobApplications
                    .CountAsync(a => a.CandidateProfileId == profile.Id
                               && a.Status == ApplicationStatus.UnderReview);

            // ── Recent Applications (last 5) ───────────────────
            var recentApps = profile == null
                ? new List<RecentApplicationDto>()
                : await _context.JobApplications
                    .Where(a => a.CandidateProfileId == profile.Id)
                    .OrderByDescending(a => a.AppliedDate)
                    .Take(5)
                    .Select(a => new RecentApplicationDto
                    {
                        JobTitle = a.Job.Title,
                        Company = a.Job.Company.Name,
                        AppliedDate = a.AppliedDate.ToRelativeDate(),
                        Status = GetStatusDisplay(a.Status)
                    })
                    .ToListAsync();

            // ── Recommended Jobs (latest 3 active jobs) ────────
            var recommendedJobs = await _context.Jobs
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.PostedDate)
                .Take(3)
                .Select(j => new RecommendedJobDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Company = j.Company.Name,
                    Location = j.Location,
                    PostedDate = j.PostedDate.ToRelativeDate(),
                    Salary = j.MinSalary != null && j.MaxSalary != null
                                     ? $"${j.MinSalary}–{j.MaxSalary}"
                                     : "N/A",
                    Description = j.Description.Length > 120
                                     ? j.Description.Substring(0, 120) + "..."
                                     : j.Description,
                    Type = j.JobType.ToString()
                })
                .ToListAsync();

            return new CandidateDashboardDto
            {
                Name = user?.FirstName ?? "User",
                ProfileCompletion = profile?.CalculateProfileCompletion(user) ?? 0,
                ProfileHints = profile?.GetProfileCompletionHints(user) ?? new List<string>(),
                TotalApplications = totalApps,
                SavedJobs = savedJobs,
                UnderReview = underReview,
                RecentApplications = recentApps,
                RecommendedJobs = recommendedJobs
            };
        }


        //-─────────── COMPANY DASHBOARD -────────────────────────────────────────────
        public async Task<CompanyDashboardDto> GetCompanyDashboardAsync(string userId)
        {
            await _jobLifecycleService.CloseExpiredJobsAsync();

            // Resolve the company profile that belongs to this user
            var companyProfile = await _context.CompanyProfiles
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (companyProfile == null)
                return new CompanyDashboardDto();

            var companyId = companyProfile.Id;
            var cutoff24h = DateTime.Now.AddHours(-24);

            // ── Stats ──────────────────────────────────────────
            var activeJobs = await _context.Jobs
                .CountAsync(j => j.CompanyProfileId == companyId && j.IsActive);

            var totalApplicants = await _context.JobApplications
                .CountAsync(a => a.Job.CompanyProfileId == companyId);

            var newApplicants = await _context.JobApplications
                .CountAsync(a => a.Job.CompanyProfileId == companyId
                             && a.AppliedDate >= cutoff24h);

            var pendingReviews = await _context.JobApplications
                .CountAsync(a => a.Job.CompanyProfileId == companyId
                             && (a.Status == ApplicationStatus.Applied
                              || a.Status == ApplicationStatus.UnderReview));

            // ── Posted Jobs (latest 6, newest first) ──────────
            var postedJobs = await _context.Jobs
                .Where(j => j.CompanyProfileId == companyId)
                .OrderByDescending(j => j.PostedDate)
                .Take(6)
                .Select(j => new PostedJobDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Location = j.Location,
                    JobType = j.JobType.ToString(),
                    Status = j.IsActive ? "Active" : "Closed",
                    ApplicantCount = j.JobApplications.Count,
                    PostedDate = j.PostedDate.ToRelativeDate()
                })
                .ToListAsync();

            // ── Recent Applicants (latest 6 across all jobs) ───
            var recentApplicants = await _context.JobApplications
                .Where(a => a.Job.CompanyProfileId == companyId)
                .OrderByDescending(a => a.AppliedDate)
                .Take(6)
                .Select(a => new RecentApplicantDto
                {
                    JobId = a.JobId,
                    ApplicationId = a.Id,
                    CandidateProfileId = a.CandidateProfileId,
                    Name = a.Candidate.User.FirstName + " " + a.Candidate.User.LastName,
                    JobTitle = a.Job.Title,
                    AppliedDate = a.AppliedDate.ToRelativeDate(),
                    Status = GetStatusDisplay(a.Status)
                })
                .ToListAsync();

            var jobList = await _context.Jobs
                .Where(j => j.CompanyProfileId == companyId)
                .OrderByDescending(j => j.PostedDate)
                .Take(6)
                .Select(j => new JobListItemDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Location = j.Location,
                    JobType = j.JobType.ToString(),
                    Department = j.Department.ToString(),
                    WorkMode = j.WorkMode.ToString(),
                    IsActive = j.IsActive,
                    IsFeatured = j.IsFeatured,
                    ApplicantCount = j.JobApplications.Count,
                    PostedDate = j.PostedDate,
                    ApplicationDeadline = j.ApplicationDeadline,
                    PostedDateDisplay = j.PostedDate.ToRelativeDate()
                })
                .ToListAsync();

            return new CompanyDashboardDto
            {
                ActiveJobs = activeJobs,
                TotalApplicants = totalApplicants,
                NewApplicants = newApplicants,
                PendingReviews = pendingReviews,
                PostedJobs = postedJobs,
                RecentApplicants = recentApplicants,
                JobItems = jobList
            };
        }

        // ─────────────────────────────── ADMIN DASHBOARD -────────────────────────────────────────────
        public async Task<AdminDashboardDto> GetAdminDashboardAsync()
        {
            // ── Role IDs ──────────────────────────────────────────
            var candidateRole = await _roleManager.FindByNameAsync(Roles.Candidate);
            var companyRole = await _roleManager.FindByNameAsync(Roles.Company);
            var adminRole = await _roleManager.FindByNameAsync(Roles.Admin);

            var candidateRoleId = candidateRole?.Id;
            var companyRoleId = companyRole?.Id;
            var adminRoleId = adminRole?.Id;

            // ── Counts ────────────────────────────────────────────
            var totalUsers = await _context.Users.CountAsync(u => !_context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == adminRoleId));

            var totalCandidates = await _context.UserRoles.CountAsync(ur => ur.RoleId == candidateRoleId);

            var totalCompanies = await _context.UserRoles.CountAsync(ur => ur.RoleId == companyRoleId);

            var activeJobs = await _context.Jobs.CountAsync(j => j.IsActive);

            var totalApplications = await _context.JobApplications.CountAsync();

            // ── Recent Users (latest 6) ─────────────────────────────
            var recentUsers = await _context.Users
                .Where(u => !_context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == adminRoleId))
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new AdminRecentUserDto
                {
                    Id = u.Id,
                    Name = u.FirstName + " " + u.LastName,
                    Email = u.Email ?? string.Empty,
                    CreatedAt = u.CreatedAt.ToDetailedDisplayDate(),
                    Role = _context.UserRoles
                                .Where(ur => ur.UserId == u.Id)
                                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                                .FirstOrDefault() ?? string.Empty
                })
                .ToListAsync();

            // ── Recent Jobs (latest 6) ─────────────────────────────
            var recentPosts = await _context.Jobs
                .OrderByDescending(j => j.PostedDate)
                .Take(6)
                .Select(j => new AdminRecentJobDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    CompanyName = j.Company.Name ?? string.Empty,
                    PostedDate = j.PostedDate.ToDetailedDisplayDate(),
                    Status = j.IsActive ? "Active" : "Closed"
                })
                .ToListAsync();

            return new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                TotalCandidates = totalCandidates,
                TotalCompanies = totalCompanies,
                ActiveJobs = activeJobs,
                TotalApplications = totalApplications,
                RecentUsers = recentUsers,
                RecentPosts = recentPosts
            };
        }


        //-─────────────────────────────── SHARED HELPERS -────────────────────────────────────────────
        private static string GetStatusDisplay(ApplicationStatus status) => status switch
        {
            ApplicationStatus.Applied => "Applied",
            ApplicationStatus.UnderReview => "Under Review",
            ApplicationStatus.Accepted => "Accepted",
            ApplicationStatus.Rejected => "Rejected",
            _ => "Unknown"
        };
    }
}
