using Masar.Core.IService;
using Masar.Domain.Enums;
using Masar.Domain.Models;
using Masar.Domain.ViewModels.CandidateDtos;
using Masar.Domain.ViewModels.CompanyDtos;
using Masar.Domain.ViewModels.JobDtos;
using Masar.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Core.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ─────────────────────────────────────────────────────
        //  CANDIDATE — apply
        // ─────────────────────────────────────────────────────

        public async Task<ApplyJobViewDto?> GetApplyViewAsync(int jobId, string userId)
        {
            var job = await _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.JobApplications)
                .Include(j => j.JobQuestions.OrderBy(q => q.Order))
                .FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive);

            if (job == null) return null;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var profile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            bool hasApplied = profile != null && await _context.JobApplications
                .AnyAsync(a => a.CandidateProfileId == profile.Id && a.JobId == jobId);

            var form = new ApplyJobDto
            {
                JobId = jobId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = profile?.PhoneNumber,
                Location = profile?.Location,
                UseExistingResume = !string.IsNullOrEmpty(profile?.ResumeUrl),
                ExistingResumeUrl = profile?.ResumeUrl
            };

            var jobDetail = new JobDetailDto
            {
                Id = job.Id,
                Title = job.Title,
                JobType = job.JobType.ToString(),
                WorkMode = job.WorkMode.ToString(),
                Location = job.Location,
                SalaryDisplay = job.MinSalary != null && job.MaxSalary != null
                    ? $"${job.MinSalary:N0}–${job.MaxSalary:N0}"
                    : job.MinSalary != null ? $"From ${job.MinSalary:N0}" : null,
                CompanyName = job.Company.Name,
                CompanyLogo = job.Company.LogoUrl,
                RequireCv = job.RequireCv,
                RequireCoverLetter = job.RequireCoverLetter,
                ApplicantCount = job.JobApplications.Count,
                ApplicationDeadline = job.ApplicationDeadline,
                IsActive = job.IsActive,
                HasApplied = hasApplied
            };

            var questions = job.JobQuestions.Select(q => new JobQuestionViewDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                Type = q.Type.ToString(),
                IsRequired = q.IsRequired,
                Order = q.Order
            }).ToList();

            return new ApplyJobViewDto
            {
                Job = jobDetail,
                Form = form,
                Questions = questions
            };
        }

        public async Task<(bool Success, string? Error)> SubmitApplicationAsync(
            int jobId, string userId, ApplyJobDto dto, string? resumeUrl)
        {
            var profile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return (false, "Candidate profile not found. Please complete your profile first.");

            var alreadyApplied = await _context.JobApplications
                .AnyAsync(a => a.CandidateProfileId == profile.Id && a.JobId == jobId);

            if (alreadyApplied)
                return (false, "You have already applied for this position.");

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive);
            if (job == null)
                return (false, "This job is no longer accepting applications.");

            _context.JobApplications.Add(new JobApplication
            {
                JobId = jobId,
                CandidateProfileId = profile.Id,
                Status = ApplicationStatus.Applied,
                AppliedDate = DateTime.UtcNow,
                ResumeUrl = resumeUrl ?? dto.ExistingResumeUrl,
                CoverLetterUrl = null
            });

            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ─────────────────────────────────────────────────────
        //  CANDIDATE — track applications
        // ─────────────────────────────────────────────────────

        public async Task<List<CandidateApplicationDto>> GetCandidateApplicationsAsync(
            string userId)
        {
            var profile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return new List<CandidateApplicationDto>();

            return await _context.JobApplications
                .Where(a => a.CandidateProfileId == profile.Id)
                .OrderByDescending(a => a.AppliedDate)
                .Select(a => new CandidateApplicationDto
                {
                    ApplicationId = a.Id,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    CompanyName = a.Job.Company.Name,
                    CompanyLogo = a.Job.Company.LogoUrl,
                    Location = a.Job.Location,
                    JobType = a.Job.JobType.ToString(),
                    SalaryDisplay = a.Job.MinSalary != null && a.Job.MaxSalary != null
                        ? $"${a.Job.MinSalary:N0}–${a.Job.MaxSalary:N0}"
                        : null,
                    Status = a.Status,
                    StatusDisplay = GetStatusDisplay(a.Status),
                    AppliedDate = a.AppliedDate,
                    AppliedDateDisplay = GetRelativeDate(a.AppliedDate)
                })
                .ToListAsync();
        }

        // ─────────────────────────────────────────────────────
        //  CANDIDATE — saved jobs
        // ─────────────────────────────────────────────────────

        public async Task<List<SavedJobDto>> GetSavedJobsAsync(string userId)
        {
            var profile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return new List<SavedJobDto>();

            return await _context.SavedJobs
                .Where(s => s.CandidateProfileId == profile.Id)
                .OrderByDescending(s => s.SavedAt)
                .Select(s => new SavedJobDto
                {
                    SavedJobId = s.Id,
                    JobId = s.JobId,
                    JobTitle = s.Job.Title,
                    CompanyName = s.Job.Company.Name,
                    CompanyLogo = s.Job.Company.LogoUrl,
                    Location = s.Job.Location,
                    JobType = s.Job.JobType.ToString(),
                    SalaryDisplay = s.Job.MinSalary != null && s.Job.MaxSalary != null
                        ? $"${s.Job.MinSalary:N0}–${s.Job.MaxSalary:N0}"
                        : null,
                    PostedDateDisplay = GetRelativeDate(s.Job.PostedDate),
                    IsActive = s.Job.IsActive
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string? Error)> ToggleSaveJobAsync(
            int jobId, string userId)
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
                SavedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ─────────────────────────────────────────────────────
        //  COMPANY — review applicants
        // ─────────────────────────────────────────────────────

        public async Task<ApplicantsViewDto?> GetApplicantsAsync(
            string userId, int jobId, string? statusFilter = null)
        {
            // Verify the job belongs to this company
            var job = await _context.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.Company.UserId == userId);

            if (job == null) return null;

            // Base query for this job's applications
            var appsQuery = _context.JobApplications
                .Where(a => a.JobId == jobId)
                .Include(a => a.Candidate)
                    .ThenInclude(c => c.User)
                .Include(a => a.Candidate)
                    .ThenInclude(c => c.CandidateSkills)
                        .ThenInclude(cs => cs.Skill)
                .AsQueryable();

            // Apply status filter if provided
            if (!string.IsNullOrEmpty(statusFilter) &&
                Enum.TryParse<ApplicationStatus>(statusFilter, out var filterStatus))
            {
                appsQuery = appsQuery.Where(a => a.Status == filterStatus);
            }

            var applications = await appsQuery
                .OrderByDescending(a => a.AppliedDate)
                .ToListAsync();

            // Count all (unfiltered) for stats
            var allApps = await _context.JobApplications
                .Where(a => a.JobId == jobId)
                .ToListAsync();

            var applicants = applications.Select(a => new ApplicantListDto
            {
                ApplicationId = a.Id,
                CandidateProfileId = a.CandidateProfileId,
                FullName = $"{a.Candidate.User.FirstName} {a.Candidate.User.LastName}",
                Email = a.Candidate.User.Email ?? string.Empty,
                PhoneNumber = a.Candidate.PhoneNumber,
                Location = a.Candidate.Location,
                ResumeUrl = a.ResumeUrl,
                CoverLetterUrl = a.CoverLetterUrl,
                Status = a.Status,
                StatusDisplay = GetStatusDisplay(a.Status),
                AppliedDate = a.AppliedDate,
                AppliedDateDisplay = GetRelativeDate(a.AppliedDate),
                Skills = a.Candidate.CandidateSkills
                    .Select(cs => cs.Skill.Name)
                    .Take(5)
                    .ToList()
            }).ToList();

            return new ApplicantsViewDto
            {
                JobId = job.Id,
                JobTitle = job.Title,
                JobLocation = job.Location,
                JobType = job.JobType.ToString(),
                IsActive = job.IsActive,
                TotalApplicants = allApps.Count,
                UnderReview = allApps.Count(a => a.Status == ApplicationStatus.UnderReview),
                Accepted = allApps.Count(a => a.Status == ApplicationStatus.Accepted),
                Rejected = allApps.Count(a => a.Status == ApplicationStatus.Rejected),
                Applicants = applicants,
                StatusFilter = statusFilter
            };
        }

        public async Task<(bool Success, string? Error)> UpdateApplicationStatusAsync(
            string userId, int applicationId, ApplicationStatus newStatus)
        {
            // Load via company ownership check
            var application = await _context.JobApplications
                .Include(a => a.Job)
                    .ThenInclude(j => j.Company)
                .FirstOrDefaultAsync(a =>
                    a.Id == applicationId &&
                    a.Job.Company.UserId == userId);

            if (application == null)
                return (false, "Application not found or access denied.");

            application.Status = newStatus;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ─────────────────────────────────────────────────────
        //  HELPERS
        // ─────────────────────────────────────────────────────

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
