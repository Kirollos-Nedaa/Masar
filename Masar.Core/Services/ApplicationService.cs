using Masar.Core.IService;
using Masar.Domain.Enums;
using Masar.Domain.Helpers;
using Masar.Domain.Models;
using Masar.Domain.ViewModels;
using Masar.Domain.ViewModels.CandidateDtos;
using Masar.Domain.ViewModels.CompanyDtos;
using Masar.Domain.ViewModels.JobDtos;
using Masar.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Masar.Core.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJobLifecycleService _jobLifecycleService;

        public ApplicationService(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IJobLifecycleService jobLifecycleService)
        {
            _context = context;
            _userManager = userManager;
            _jobLifecycleService = jobLifecycleService;
        }

        // ─────────────────────────────────────────────────────
        //  CANDIDATE
        // ─────────────────────────────────────────────────────

        public async Task<ApplyJobViewDto?> GetApplyViewAsync(int jobId, string userId)
        {
            await _jobLifecycleService.CloseExpiredJobsAsync();

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
                ExistingResumeUrl = profile?.ResumeUrl,
                ExistingResumeName = profile?.ResumeOriginalName
            };

            var jobDetail = new JobDetailDto
            {
                Id = job.Id,
                Title = job.Title,
                JobType = job.JobType == JobType.FullTime ? "Full-time"
                        : job.JobType == JobType.PartTime ? "Part-time"
                        : job.JobType == JobType.Internship ? "Internship"
                        : job.JobType.ToString(),
                WorkMode = job.WorkMode.ToString(),
                Location = job.Location,
                SalaryDisplay = job.MinSalary.ToSalaryDisplay(job.MaxSalary),
                CompanyName = job.Company.Name,
                CompanyLogo = job.Company.LogoUrl,
                RequireCv = job.RequireCv,
                RequireCoverLetter = job.RequireCoverLetter,
                ApplicantCount = job.JobApplications.Count,
                ApplicationDeadline = job.ApplicationDeadline,
                IsActive = job.IsActive,
                HasApplied = hasApplied
            };

            var questions = job.JobQuestions
                .Where(q => q.IsActive)
                .OrderBy(q => q.Order)
                .Select(q => new JobQuestionViewDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Type = q.Type.ToString(),
                    IsRequired = q.IsRequired,
                    Order = q.Order
                })
                .ToList();

            return new ApplyJobViewDto
            {
                Job = jobDetail,
                Form = form,
                Questions = questions
            };
        }

        public async Task<(bool Success, string? Error)> SubmitApplicationAsync(
            int jobId, string userId, ApplyJobDto dto, string? uploadedResumeUrl)
        {
            await _jobLifecycleService.CloseExpiredJobsAsync();

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

            string? finalResumeUrl = null;

            if (uploadedResumeUrl != null)
            {
                finalResumeUrl = uploadedResumeUrl;
            }
            else if (dto.UseExistingResume && !string.IsNullOrEmpty(dto.ExistingResumeUrl))
            {
                finalResumeUrl = dto.ExistingResumeUrl;
            }

            var application = new JobApplication
            {
                JobId = jobId,
                CandidateProfileId = profile.Id,
                Status = ApplicationStatus.Applied,
                AppliedDate = DateTime.UtcNow,
                ResumeUrl = finalResumeUrl,
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

        public async Task<List<CandidateApplicationDto>> GetCandidateApplicationsAsync(string userId)
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
                    SalaryDisplay = a.Job.MinSalary.ToSalaryDisplay(a.Job.MaxSalary),
                    Status = a.Status,
                    StatusDisplay = GetStatusDisplay(a.Status),
                    AppliedDate = a.AppliedDate,
                    AppliedDateDisplay = a.AppliedDate.ToRelativeDate()
                })
                .ToListAsync();
        }

        // ─────────────────────────────────────────────────────
        //  COMPANY
        // ─────────────────────────────────────────────────────

        public async Task<ApplicantsViewDto?> GetApplicantsAsync(int jobId, string companyUserId, string? searchQuery = null, string? statusFilter = null, string? sortFilter = null, int page = 1, int pageSize = 6)
        {
            var job = await _context.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.Company.UserId == companyUserId);

            if (job == null) return null;

            var allStatuses = await _context.JobApplications
                .Where(a => a.JobId == jobId)
                .Select(a => a.Status)
                .ToListAsync();

            int total = allStatuses.Count;
            int accepted = allStatuses.Count(s => s == ApplicationStatus.Accepted);
            int underReview = allStatuses.Count(s => s == ApplicationStatus.UnderReview);
            int rejected = allStatuses.Count(s => s == ApplicationStatus.Rejected);

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

            query = sortFilter switch
            {
                "oldest" => query.OrderBy(a => a.AppliedDate),
                _ => query.OrderByDescending(a => a.AppliedDate)
            };

            pageSize = pageSize <= 0 ? 6 : pageSize;

            var totalCount = await query.CountAsync();
            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);
            page = totalPages > 0
                ? Math.Min(Math.Max(page, 1), totalPages)
                : 1;

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
                    ResumeUrl = a.ResumeUrl,
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

        public async Task<(bool Success, string? Error)> UpdateApplicationStatusAsync(
            string userId, int applicationId, ApplicationStatus newStatus)
        {
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

            if (application.Status == ApplicationStatus.Applied)
            {
                application.Status = ApplicationStatus.UnderReview;
                await _context.SaveChangesAsync();
            }

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
                ResumeUrl = application.ResumeUrl,
                CoverLetter = application.CoverLetter,
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
    }
}