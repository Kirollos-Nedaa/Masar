using Masar.Core.IService;
using Masar.Domain.Enums;
using Masar.Domain.Models;
using Masar.Domain.ViewModels;
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

        public async Task<(bool Success, string? Error)> SubmitApplicationAsync(int jobId, string userId, ApplyJobDto dto, string? resumeUrl)
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

            var application = new JobApplication
            {
                JobId = jobId,
                CandidateProfileId = profile.Id,
                Status = ApplicationStatus.Applied,
                AppliedDate = DateTime.UtcNow,
                ResumeUrl = resumeUrl ?? dto.ExistingResumeUrl,
                CoverLetter = dto.CoverLetter
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            if (dto.Answers is { Count: > 0 })
            {
                var answers = dto.Answers.Select(a => new ApplicationAnswer
                {
                    JobApplicationId = application.Id,
                    JobQuestionId = a.QuestionId,
                    AnswerText = a.AnswerText ?? string.Empty
                });

                _context.ApplicationAnswers.AddRange(answers);
                await _context.SaveChangesAsync();
            }

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

        public async Task<ApplicantsViewDto?> GetApplicantsAsync(int jobId, string companyUserId, string? searchQuery = null, string? statusFilter = null, string? sortFilter = null, int page = 1, int pageSize = 6)
        {
            // 1. Verify ownership
            var job = await _context.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.Company.UserId == companyUserId);

            if (job == null) return null;

            // 2. Stat counts — always over ALL applicants, ignoring any active filter
            var allStatuses = await _context.JobApplications
                .Where(a => a.JobId == jobId)
                .Select(a => a.Status)
                .ToListAsync();

            int total = allStatuses.Count;
            int accepted = allStatuses.Count(s => s == ApplicationStatus.Accepted);
            int underReview = allStatuses.Count(s => s == ApplicationStatus.UnderReview);
            int rejected = allStatuses.Count(s => s == ApplicationStatus.Rejected);

            // 3. Filtered query for the card list
            var query = _context.JobApplications
                .Where(a => a.JobId == jobId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var q = searchQuery.Trim().ToLower();
                query = query.Where(a =>
                    (a.Candidate.User.FirstName + " " + a.Candidate.User.LastName).ToLower().Contains(q) ||
                    a.Candidate.User.Email.ToLower().Contains(q));
            }

            if (!string.IsNullOrWhiteSpace(statusFilter) &&
                Enum.TryParse<ApplicationStatus>(statusFilter, out var parsedStatus) &&
                parsedStatus != ApplicationStatus.None)
            {
                query = query.Where(a => a.Status == parsedStatus);
            }

            // 4. Sort
            query = sortFilter switch
            {
                "oldest" => query.OrderBy(a => a.AppliedDate),
                _ => query.OrderByDescending(a => a.AppliedDate)   // default: most recent
            };

            pageSize = pageSize <= 0 ? 6 : pageSize;

            var totalCount = await query.CountAsync();
            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);
            page = totalPages > 0
                ? Math.Min(Math.Max(page, 1), totalPages)
                : 1;

            // 5. Project to DTO
            var cards = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ApplicantCardDto
                {
                    ApplicationId = a.Id,
                    CandidateProfileId = a.CandidateProfileId,
                    FullName = a.Candidate.User.FirstName + " " + a.Candidate.User.LastName,
                    Email = a.Candidate.User.Email ?? string.Empty,
                    PhoneNumber = a.Candidate.PhoneNumber,
                    Location = a.Candidate.Location,
                    Status = a.Status.ToString(),
                    AppliedDate = a.AppliedDate,
                    Skills = a.Candidate.CandidateSkills
                                            .Select(cs => cs.Skill.Name)
                                            .Take(6)
                                            .ToList(),
                    LatestEducation = a.Candidate.Educations
                                            .OrderByDescending(e => e.ExpectedGraduation)
                                            .Select(e => e.University + " - " + e.Degree + " " + e.Major)
                                            .FirstOrDefault(),
                    ResumeUrl = a.ResumeUrl ?? a.Candidate.ResumeUrl,
                    professionalLinks = a.Candidate.ProfessionalLinks
                                            .Select(pl => new ProfessionalLinkDto
                                            {
                                                Id = pl.Id,
                                                Url = pl.Url,
                                                LinkName = pl.LinksNames
                                            })
                                            .ToList()
                })
                .ToListAsync();

            return new ApplicantsViewDto
            {
                JobId = job.Id,
                JobTitle = job.Title,
                NumberOfOpenings = job.NumberOfOpenings,
                Applicants = cards,
                TotalApplicants = total,
                AcceptedCount = accepted,
                UnderReviewCount = underReview,
                RejectedCount = rejected,
                SearchQuery = searchQuery,
                StatusFilter = statusFilter,
                SortFilter = sortFilter,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<(bool Success, string? Error)> UpdateApplicationStatusAsync(string userId, int applicationId, ApplicationStatus newStatus)
        {
            // Load via company ownership check
            var application = await _context.JobApplications
                .Include(a => a.Job)
                .ThenInclude(j => j.Company)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.Job.Company.UserId == userId);

            if (application == null)
                return (false, "Application not found or access denied.");

            application.Status = newStatus;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<ReviewApplicationViewDto?> StartReviewAsync(int applicationId, string companyUserId)
        {
            var application = await _context.JobApplications
                .Include(a => a.Job)
                    .ThenInclude(j => j.Company)
                .Include(a => a.Job)
                    .ThenInclude(j => j.JobQuestions.OrderBy(q => q.Order))
                .Include(a => a.Candidate)
                    .ThenInclude(c => c.User)
                .Include(a => a.Candidate)
                    .ThenInclude(c => c.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
                .Include(a => a.Candidate)
                    .ThenInclude(c => c.Educations)
                .Include(a => a.Candidate)
                    .ThenInclude(c => c.ProfessionalLinks)
                .Include(a => a.Answers)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.Job.Company.UserId == companyUserId);

            if (application is null) return null;

            // Advance status: Applied → UnderReview (never go backwards)
            if (application.Status == ApplicationStatus.Applied)
            {
                application.Status = ApplicationStatus.UnderReview;
                await _context.SaveChangesAsync();
            }

            var links = application.Candidate.ProfessionalLinks;

            // Build answer list matched to question text
            var answers = application.Job.JobQuestions
                .Select(q =>
                {
                    var answer = application.Answers.FirstOrDefault(a => a.JobQuestionId == q.Id);
                    return new ReviewAnswerDto
                    {
                        QuestionText = q.QuestionText,
                        QuestionType = q.Type.ToString(),
                        Answer = answer?.AnswerText ?? "(no answer)",
                        Order = q.Order
                    };
                })
                .OrderBy(a => a.Order)
                .ToList();

            return new ReviewApplicationViewDto
            {
                ApplicationId = application.Id,
                JobId = application.JobId,
                JobTitle = application.Job.Title,
                CandidateProfileId = application.CandidateProfileId,
                FullName = $"{application.Candidate.User.FirstName} {application.Candidate.User.LastName}",
                Email = application.Candidate.User.Email ?? string.Empty,
                PhoneNumber = application.Candidate.PhoneNumber,
                Location = application.Candidate.Location,
                Bio = application.Candidate.Bio,
                Gender = application.Candidate.Gender.ToString(),
                Status = GetStatusDisplay(application.Status),
                AppliedDate = application.AppliedDate,
                Skills = application.Candidate.CandidateSkills
                    .Select(cs => cs.Skill.Name)
                    .ToList(),
                Educations = application.Candidate.Educations
                    .OrderByDescending(e => e.ExpectedGraduation)
                    .Select(e => new ReviewEducationDto
                    {
                        University = e.University,
                        Degree = e.Degree,
                        Major = e.Major,
                        Years = $"{e.StartYear.Year} – {e.ExpectedGraduation.Year}"
                    })
                    .ToList(),
                Answers = answers,
                ResumeUrl = application.ResumeUrl ?? application.Candidate.ResumeUrl,
                CoverLetter = application.CoverLetter.ToString(),
                professionalLinks = application.Candidate.ProfessionalLinks
                    .Select(pl => new ProfessionalLinkDto
                    {
                        Id = pl.Id,
                        Url = pl.Url,
                        LinkName = pl.LinksNames
                    })
                    .ToList()
            };
        }

        public async Task<bool> AcceptApplicationAsync(int applicationId, string companyUserId)
        {
            var application = await _context.JobApplications
                .Include(a => a.Job)
                .ThenInclude(j => j.Company)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.Job.Company.UserId == companyUserId);

            if (application is null) return false;

            if (application.Status == ApplicationStatus.Accepted) return true;

            application.Status = ApplicationStatus.Accepted;

            var job = application.Job;

            if (job.NumberOfOpenings > 0)
                job.NumberOfOpenings--;

            // If openings hit 0 → reject every other pending applicant for this job
            if (job.NumberOfOpenings == 0)
            {
                var pendingOthers = await _context.JobApplications
                    .Where(a => a.JobId == job.Id && a.Id != applicationId && 
                        (a.Status == ApplicationStatus.Applied || a.Status == ApplicationStatus.UnderReview))
                    .ToListAsync();

                foreach (var other in pendingOthers)
                    other.Status = ApplicationStatus.Rejected;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectApplicationAsync(int applicationId, string companyUserId)
        {
            var application = await _context.JobApplications
                .Include(a => a.Job)
                .ThenInclude(j => j.Company)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.Job.Company.UserId == companyUserId);

            if (application is null) return false;

            application.Status = ApplicationStatus.Rejected;
            await _context.SaveChangesAsync();
            return true;
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
