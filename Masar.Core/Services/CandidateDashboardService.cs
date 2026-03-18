using Masar.Core.IService;
using Masar.Domain.Enums;
using Masar.Domain.Models;
using Masar.Domain.ViewModels;
using Masar.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Core.Services
{
    public class CandidateDashboardService : ICandidateDashboardService
    {
        private readonly AppDbContext _context;

        public CandidateDashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CandidateDashboardDto> GetDashboardAsync(string userId)
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
                        Company = a.Job.Company.CompanyName,
                        AppliedDate = GetRelativeDate(a.AppliedDate),
                        Status = GetStatusDisplay(a.Status)
                    })
                    .ToListAsync();

            // ── Recommended Jobs (latest active, simple version) ─
            var recommendedJobs = await _context.Jobs
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.PostedDate)
                .Take(3)
                .Select(j => new RecommendedJobDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Company = j.Company.CompanyName,
                    Location = j.Location,
                    PostedDate = GetRelativeDate(j.PostedDate),
                    Salary = j.MinSalary != null && j.MaxSalary != null
                                    ? $"${j.MinSalary}–{j.MaxSalary}"
                                    : "N/A",
                    Description = j.Description.Length > 120
                                    ? j.Description.Substring(0, 120) + "..."
                                    : j.Description,
                    Type = j.JobType
                })
                .ToListAsync();

            return new CandidateDashboardDto
            {
                Name = user?.FirstName ?? "User",
                ProfileCompletion = CalculateProfileCompletion(user, profile),
                TotalApplications = totalApps,
                SavedJobs = savedJobs,
                UnderReview = underReview,
                RecentApplications = recentApps,
                RecommendedJobs = recommendedJobs
            };
        }

        // ── Helpers ────────────────────────────────────────────
        private static int CalculateProfileCompletion(ApplicationUser? user, CandidateProfile? profile)
        {
            if (user == null || profile == null) return 0;

            // 5 sections × 20 points each = 100
            int score = 0;

            // 1. Personal info
            if (!string.IsNullOrEmpty(user.FirstName)
             && !string.IsNullOrEmpty(user.LastName)
             && !string.IsNullOrEmpty(user.PhoneNumber)
             && !string.IsNullOrEmpty(profile.Location)
             && !string.IsNullOrEmpty(profile.Bio))
                score += 20;

            // 2. Education
            if (profile.Educations?.Any() == true)
                score += 20;

            // 3. Skills
            if (profile.CandidateSkills?.Any() == true)
                score += 20;

            // 4. Resume
            if (!string.IsNullOrEmpty(profile.ResumeUrl))
                score += 20;

            // 5. Professional links
            var links = profile.ProfessionalLinks;
            if (links != null && (
                   !string.IsNullOrEmpty(links.LinkedInUrl)
                || !string.IsNullOrEmpty(links.GitHubUrl)
                || !string.IsNullOrEmpty(links.PortfolioUrl)))
                score += 20;

            return score;
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
