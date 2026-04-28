using Masar.Core.IService;
using Masar.Domain.Enums;
using Masar.Domain.Models;
using Masar.Domain.ViewModels.CompanyDtos;
using Masar.Domain.ViewModels.Job;
using Masar.Domain.ViewModels.JobDtos;
using Masar.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Masar.Core.Services
{
    public class JobService : IJobService
    {
        private readonly AppDbContext _context;

        public JobService(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────
        //  COMPANY — manage jobs
        // ─────────────────────────────────────────────────────

        public async Task<int> PostJobAsync(string userId, PostJobDto dto)
        {
            var company = await _context.CompanyProfiles
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (company == null)
                throw new InvalidOperationException("Company profile not found.");

            var job = new Job
            {
                CompanyProfileId = company.Id,
                Title = dto.Title,
                JobType = dto.JobType,
                Department = dto.Department,
                Location = dto.Location,
                WorkMode = dto.WorkMode,
                MinSalary = dto.MinSalary,
                MaxSalary = dto.MaxSalary,
                Description = dto.Description,
                Requirements = dto.Requirements,
                Benefits = dto.Benefits,
                ApplicationDeadline = dto.ApplicationDeadline,
                NumberOfOpenings = dto.NumberOfOpenings,
                RequireCv = dto.RequireCv,
                RequireCoverLetter = dto.RequireCoverLetter,
                IsActive = true,
                IsFeatured = false,
                PostedDate = DateTime.UtcNow
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            // Save questions
            await SyncQuestionsAsync(job.Id, dto.Questions);

            return job.Id;
        }

        public async Task<bool> UpdateJobAsync(string userId, int jobId, PostJobDto dto)
        {
            var job = await GetOwnedJobAsync(userId, jobId);
            if (job == null) return false;

            job.Title = dto.Title;
            job.JobType = dto.JobType;
            job.Department = dto.Department;
            job.Location = dto.Location;
            job.WorkMode = dto.WorkMode;
            job.MinSalary = dto.MinSalary;
            job.MaxSalary = dto.MaxSalary;
            job.Description = dto.Description;
            job.Requirements = dto.Requirements;
            job.Benefits = dto.Benefits;
            job.ApplicationDeadline = dto.ApplicationDeadline;
            job.NumberOfOpenings = dto.NumberOfOpenings;
            job.RequireCv = dto.RequireCv;
            job.RequireCoverLetter = dto.RequireCoverLetter;

            await _context.SaveChangesAsync();

            // Sync questions (delete old, insert new)
            await SyncQuestionsAsync(job.Id, dto.Questions);

            return true;
        }

        public async Task<bool> DeleteJobAsync(string userId, int jobId)
        {
            var job = await GetOwnedJobAsync(userId, jobId);
            if (job == null) return false;

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleJobStatusAsync(string userId, int jobId)
        {
            var job = await GetOwnedJobAsync(userId, jobId);
            if (job == null) return false;

            job.IsActive = !job.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PostJobDto?> GetJobForEditAsync(string userId, int jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.JobQuestions)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.Company.UserId == userId);

            if (job == null) return null;

            return new PostJobDto
            {
                Title = job.Title,
                JobType = job.JobType,
                Department = job.Department,
                Location = job.Location,
                WorkMode = job.WorkMode,
                MinSalary = job.MinSalary,
                MaxSalary = job.MaxSalary,
                Description = job.Description,
                Requirements = job.Requirements,
                Benefits = job.Benefits,
                ApplicationDeadline = job.ApplicationDeadline,
                NumberOfOpenings = job.NumberOfOpenings,
                RequireCv = job.RequireCv,
                RequireCoverLetter = job.RequireCoverLetter,
                Questions = job.JobQuestions
                    .OrderBy(q => q.Order)
                    .Select(q => new JobQuestionDto
                    {
                        Id = q.Id,
                        QuestionText = q.QuestionText,
                        Type = q.Type.ToString(),
                        IsRequired = q.IsRequired,
                        Order = q.Order
                    })
                    .ToList()
            };
        }

        public async Task<CompanyJobsViewDto> GetCompanyJobsAsync(string userId, int page = 1, int pageSize = 10)
        {
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var company = await _context.CompanyProfiles
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (company == null)
                return new CompanyJobsViewDto { Page = 1, PageSize = pageSize };

            var query = _context.Jobs
                .Where(j => j.CompanyProfileId == company.Id)
                .OrderByDescending(j => j.PostedDate);

            var totalCount = await query.CountAsync();
            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);
            page = totalPages > 0
                ? Math.Min(Math.Max(page, 1), totalPages)
                : 1;

            var jobs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                    PostedDateDisplay = GetRelativeDate(j.PostedDate)
                })
                .ToListAsync();

            return new CompanyJobsViewDto
            {
                Jobs = jobs,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        // ─────────────────────────────────────────────────────
        //  CANDIDATE — browse jobs
        // ─────────────────────────────────────────────────────

        public async Task<JobBrowseResultDto> BrowseJobsAsync(
            JobFilterDto filter, string? candidateUserId = null)
        {
            var query = _context.Jobs
                .Include(j => j.Company)
                .Where(j => j.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.ToLower();
                query = query.Where(j =>
                    j.Title.ToLower().Contains(s) ||
                    j.Company.Name.ToLower().Contains(s) ||
                    j.Description.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(filter.Location))
            {
                var loc = filter.Location.ToLower();
                query = query.Where(j => j.Location.ToLower().Contains(loc));
            }

            if (filter.JobTypes.Any())
            {
                var types = filter.JobTypes
                    .Select(t => Enum.TryParse<JobType>(t, true, out var jt) ? (JobType?)jt : null)
                    .Where(t => t.HasValue)
                    .Select(t => t!.Value)
                    .ToList();

                if (types.Any())
                    query = query.Where(j => types.Contains(j.JobType));
            }

            if (filter.Industries.Any())
            {
                query = query.Where(j =>
                    j.Company.Industry != null &&
                    filter.Industries.Contains(j.Company.Industry));
            }

            if (!string.IsNullOrWhiteSpace(filter.SalaryRange))
            {
                switch (filter.SalaryRange)
                {
                    case "0-50000": query = query.Where(j => j.MinSalary <= 50000); break;
                    case "50000-100000": query = query.Where(j => j.MinSalary >= 50000 && j.MinSalary <= 100000); break;
                    case "100000-150000": query = query.Where(j => j.MinSalary >= 100000 && j.MinSalary <= 150000); break;
                    case "150000+": query = query.Where(j => j.MinSalary >= 150000); break;
                }
            }

            query = filter.SortBy switch
            {
                "salary_desc" => query.OrderByDescending(j => j.MaxSalary),
                "salary_asc" => query.OrderBy(j => j.MinSalary),
                _ => query.OrderByDescending(j => j.PostedDate)
            };

            var totalCount = await query.CountAsync();

            HashSet<int> savedJobIds = new();
            if (!string.IsNullOrEmpty(candidateUserId))
            {
                var profile = await _context.CandidateProfiles
                    .FirstOrDefaultAsync(p => p.UserId == candidateUserId);

                if (profile != null)
                {
                    savedJobIds = (await _context.SavedJobs
                        .Where(s => s.CandidateProfileId == profile.Id)
                        .Select(s => s.JobId)
                        .ToListAsync())
                        .ToHashSet();
                }
            }

            var jobs = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(j => new JobBrowseDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    CompanyName = j.Company.Name,
                    CompanyLogo = j.Company.LogoUrl,
                    Location = j.Location,
                    JobType = j.JobType.ToString(),
                    WorkMode = j.WorkMode.ToString(),
                    Department = j.Department.ToString(),
                    Industry = j.Company.Industry,
                    PostedDateDisplay = GetRelativeDate(j.PostedDate),
                    SalaryDisplay = j.MinSalary != null && j.MaxSalary != null
                        ? $"${j.MinSalary:N0}–${j.MaxSalary:N0}"
                        : j.MinSalary != null ? $"From ${j.MinSalary:N0}" : null,
                    DescriptionSnippet = j.Description.Length > 150
                        ? j.Description.Substring(0, 150) + "..."
                        : j.Description
                })
                .ToListAsync();

            foreach (var job in jobs)
                job.IsSaved = savedJobIds.Contains(job.Id);

            return new JobBrowseResultDto
            {
                Jobs = jobs,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                Filter = filter
            };
        }

        public async Task<JobDetailDto?> GetJobDetailAsync(
            int jobId, string? candidateUserId = null)
        {
            var job = await _context.Jobs
                .Include(j => j.Company)
                    .ThenInclude(c => c.ContactInfo)
                .Include(j => j.JobApplications)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null) return null;

            bool isSaved = false;
            bool hasApplied = false;

            if (!string.IsNullOrEmpty(candidateUserId))
            {
                var profile = await _context.CandidateProfiles
                    .FirstOrDefaultAsync(p => p.UserId == candidateUserId);

                if (profile != null)
                {
                    isSaved = await _context.SavedJobs
                        .AnyAsync(s => s.CandidateProfileId == profile.Id && s.JobId == jobId);

                    hasApplied = await _context.JobApplications
                        .AnyAsync(a => a.CandidateProfileId == profile.Id && a.JobId == jobId);
                }
            }

            return new JobDetailDto
            {
                Id = job.Id,
                Title = job.Title,
                JobType = job.JobType.ToString(),
                WorkMode = job.WorkMode.ToString(),
                Department = job.Department.ToString(),
                Location = job.Location,
                Description = job.Description,
                Requirements = job.Requirements,
                Benefits = job.Benefits,
                PostedDateDisplay = GetRelativeDate(job.PostedDate),
                SalaryDisplay = job.MinSalary != null && job.MaxSalary != null
                    ? $"${job.MinSalary:N0}–${job.MaxSalary:N0}"
                    : job.MinSalary != null ? $"From ${job.MinSalary:N0}" : null,
                ApplicantCount = job.JobApplications.Count,
                NumberOfOpenings = job.NumberOfOpenings,
                ApplicationDeadline = job.ApplicationDeadline,
                RequireCv = job.RequireCv,
                RequireCoverLetter = job.RequireCoverLetter,
                IsActive = job.IsActive,
                CompanyProfileId = job.Company.Id,
                CompanyName = job.Company.Name,
                CompanyLogo = job.Company.LogoUrl,
                CompanyDescription = job.Company.Description,
                CompanyIndustry = job.Company.Industry,
                CompanySize = job.Company.Size?.ToString(),
                CreatedAt = job.Company.CreatedAt.ToString("yyyy"),
                IsSaved = isSaved,
                HasApplied = hasApplied
            };
        }

        // ─────────────────────────────────────────────────────
        //  HELPERS
        // ─────────────────────────────────────────────────────

        private async Task SyncQuestionsAsync(int jobId, List<JobQuestionDto> questions)
        {
            // Remove all existing questions for this job
            var existing = await _context.JobQuestions
                .Where(q => q.JobId == jobId)
                .ToListAsync();

            _context.JobQuestions.RemoveRange(existing);

            // Add the new set
            for (int i = 0; i < questions.Count; i++)
            {
                var q = questions[i];
                if (string.IsNullOrWhiteSpace(q.QuestionText)) continue;

                _context.JobQuestions.Add(new JobQuestion
                {
                    JobId = jobId,
                    QuestionText = q.QuestionText,
                    Type = Enum.TryParse<QuestionType>(q.Type, out var qt)
                                       ? qt : QuestionType.Essay,
                    IsRequired = q.IsRequired,
                    Order = i
                });
            }

            await _context.SaveChangesAsync();
        }

        private async Task<Job?> GetOwnedJobAsync(string userId, int jobId)
        {
            return await _context.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.Company.UserId == userId);
        }

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
