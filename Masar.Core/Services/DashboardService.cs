// Masar.Core/Services/DashboardService.cs

using Humanizer;
using Masar.Core.IService;
using Masar.Domain.Enums;
using Masar.Domain.Models;
using Masar.Domain.ViewModels;
using Masar.Domain.ViewModels.CandidateDtos;
using Masar.Domain.ViewModels.CompanyDtos;
using Masar.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Masar.Core.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly IProfileService _profileService;

        public DashboardService(AppDbContext context, IProfileService profileService)
        {
            _context = context;
            _profileService = profileService;
        }


        //-───────────── CANDIDATE DASHBOARD -────────────────────────────────────────────
        public async Task<CandidateDashboardDto> GetCandidateDashboardAsync(string userId)
        {
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
                        AppliedDate = GetRelativeDate(a.AppliedDate),
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
                    PostedDate = GetRelativeDate(j.PostedDate),
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
                ProfileCompletion = CalculateProfileCompletion(user, profile),
                ProfileHints = GetProfileCompletionHints(user, profile),
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
            // Resolve the company profile that belongs to this user
            var companyProfile = await _context.CompanyProfiles
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (companyProfile == null)
                return new CompanyDashboardDto();

            var companyId = companyProfile.Id;
            var cutoff24h = DateTime.UtcNow.AddHours(-24);

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

            // ── Posted Jobs (last 5, newest first) ────────────
            var postedJobs = await _context.Jobs
                .Where(j => j.CompanyProfileId == companyId)
                .OrderByDescending(j => j.PostedDate)
                .Take(5)
                .Select(j => new PostedJobDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Location = j.Location,
                    JobType = j.JobType.ToString(),
                    Status = j.IsActive ? "Active" : "Closed",
                    ApplicantCount = j.JobApplications.Count,
                    PostedDate = GetRelativeDate(j.PostedDate)
                })
                .ToListAsync();

            // ── Recent Applicants (last 5 across all jobs) ─────
            var recentApplicants = await _context.JobApplications
                .Where(a => a.Job.CompanyProfileId == companyId)
                .OrderByDescending(a => a.AppliedDate)
                .Take(5)
                .Select(a => new RecentApplicantDto
                {
                    JobId = a.JobId,
                    Name = a.Candidate.User.FirstName + " " + a.Candidate.User.LastName,
                    JobTitle = a.Job.Title,
                    AppliedDate = GetRelativeDate(a.AppliedDate),
                    Status = GetStatusDisplay(a.Status)
                })
                .ToListAsync();

            return new CompanyDashboardDto
            {
                ActiveJobs = activeJobs,
                TotalApplicants = totalApplicants,
                NewApplicants = newApplicants,
                PendingReviews = pendingReviews,
                PostedJobs = postedJobs,
                RecentApplicants = recentApplicants
            };
        }


        //-─────────────────────────────── SHARED HELPERS -────────────────────────────────────────────
        private static int CalculateProfileCompletion(ApplicationUser? user, CandidateProfile? profile)
        {
            if (user == null || profile == null) return 0;

            int score = 0;

            // Personal info — 30 pts
            bool hasAllPersonalInfo =
                !string.IsNullOrWhiteSpace(user.FirstName) &&
                !string.IsNullOrWhiteSpace(user.LastName) &&
                !string.IsNullOrWhiteSpace(user.Email) &&
                profile.Gender.HasValue &&
                !string.IsNullOrWhiteSpace(profile.Location) &&
                profile.DateOfBirth.HasValue &&
                !string.IsNullOrWhiteSpace(profile.PhoneNumber) &&
                !string.IsNullOrWhiteSpace(profile.Bio);

            if (hasAllPersonalInfo)
                score += 30;

            // Education — 30 pts
            if (profile.Educations?.Any() == true)
                score += 30;

            // Skills — 20 pts
            if (profile.CandidateSkills?.Any() == true)
                score += 20;

            // Professional links — 20 pts
            if (profile.ProfessionalLinks?.Any() == true)
                score += 20;

            return score;
        }

        private static List<string> GetProfileCompletionHints(ApplicationUser user, CandidateProfile profile)
        {
            var hints = new List<string>();

            // Personal Info (ALL required)
            bool hasAllPersonalInfo =
                !string.IsNullOrWhiteSpace(user.FirstName) &&
                !string.IsNullOrWhiteSpace(user.LastName) &&
                !string.IsNullOrWhiteSpace(user.Email) &&
                profile.Gender.HasValue &&
                !string.IsNullOrWhiteSpace(profile.Location) &&
                profile.DateOfBirth.HasValue &&
                !string.IsNullOrWhiteSpace(profile.PhoneNumber) &&
                !string.IsNullOrWhiteSpace(profile.Bio);

            if (!hasAllPersonalInfo)
                hints.Add("Complete your personal information");

            // Education
            if (profile.Educations?.Any() != true)
                hints.Add("Add your education");

            // Skills
            if (profile.CandidateSkills?.Any() != true)
                hints.Add("Add your skills");

            // Professional Links
            if (profile.ProfessionalLinks?.Any() != true)
                hints.Add("Add professional links");

            return hints;
        }

        private static string GetStatusDisplay(ApplicationStatus status) => status switch
        {
            ApplicationStatus.Applied => "Applied",
            ApplicationStatus.UnderReview => "Under Review",
            ApplicationStatus.Accepted => "Accepted",
            ApplicationStatus.Rejected => "Rejected",
            _ => "Unknown"
        };

        private static string GetRelativeDate(DateTime date)
        {
            var diff = DateTime.UtcNow - date;
            if (diff.TotalDays < 1) return "Today";
            if (diff.TotalDays < 2) return "Yesterday";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} days ago";
            if (diff.TotalDays < 30) return $"{(int)(diff.TotalDays / 7)} week(s) ago";
            return date.ToString("MMM dd, yyyy");
        }
    }
}