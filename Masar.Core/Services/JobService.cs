using Masar.Core.IService;
using Masar.Domain.Enums;
using Masar.Domain.Helpers;
using Masar.Domain.Models;
using Masar.Domain.ViewModels;
using Masar.Domain.ViewModels.CompanyDtos;
using Masar.Domain.ViewModels.Job;
using Masar.Domain.ViewModels.JobDtos;
using Masar.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Masar.Core.Services
{
    public class JobService : IJobService
    {
        private readonly AppDbContext _context;
        private readonly IJobLifecycleService _jobLifecycleService;

        public JobService(AppDbContext context, IJobLifecycleService jobLifecycleService)
        {
            _context = context;
            _jobLifecycleService = jobLifecycleService;
        }

        // ─────────────────────────────────────────────────────
        //  COMPANY — manage jobs
        // ─────────────────────────────────────────────────────

        public async Task<int> PostJobAsync(string userId, PostJobDto dto)
        {
            EnsureDeadlineIsInFuture(dto.ApplicationDeadline);

            var company = await _context.CompanyProfiles
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (company == null)
                throw new InvalidOperationException("Company profile not found.");

            var job = new Job
            {
                CompanyProfileId = company.Id,
                Title = dto.Title,
                JobType = dto.JobType,
                Department = dto.Department, // Saved cleanly as Department enum
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
            EnsureDeadlineIsInFuture(dto.ApplicationDeadline);

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
            await _jobLifecycleService.CloseExpiredJobsAsync();

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
                    .Where(q => q.IsActive)
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
            await _jobLifecycleService.CloseExpiredJobsAsync();

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
                    PostedDateDisplay = j.PostedDate.ToRelativeDate()
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

        public async Task<JobBrowseResultDto> BrowseJobsAsync(JobFilterDto filter, string? candidateUserId = null)
        {
            await _jobLifecycleService.CloseExpiredJobsAsync();

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

            // 1. New Department Filter
            if (filter.Departments != null && filter.Departments.Any())
            {
                var parsedDepartments = filter.Departments
                    .Select(d => Enum.TryParse<Department>(d, true, out var dept) ? (Department?)dept : null)
                    .Where(d => d.HasValue)
                    .Select(d => d!.Value)
                    .ToList();

                if (parsedDepartments.Any())
                {
                    query = query.Where(j => parsedDepartments.Contains(j.Department));
                }
            }

            // 2. Updated Industry Filter (Macro: Applies to the Company hosting the Job)
            if (filter.Industries != null && filter.Industries.Any())
            {
                var parsedIndustries = filter.Industries
                    .Select(i => Enum.TryParse<Industries>(i, true, out var ind) ? (Industries?)ind : null)
                    .Where(i => i.HasValue)
                    .Select(i => i!.Value)
                    .ToList();

                if (parsedIndustries.Any())
                {
                    query = query.Where(j => j.Company.Industry != null && filter.Industries.Contains(j.Company.Industry));
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.SalaryRange))
            {
                if (filter.SalaryRange.EndsWith("+"))
                {
                    var cleanValue = filter.SalaryRange.TrimEnd('+');
                    if (decimal.TryParse(cleanValue, out decimal userMinBoundary))
                    {
                        query = query.Where(j => (j.MaxSalary ?? j.MinSalary) >= userMinBoundary);
                    }
                }
                else if (filter.SalaryRange.Contains("-"))
                {
                    var parts = filter.SalaryRange.Split('-');
                    if (parts.Length == 2 &&
                        decimal.TryParse(parts[0], out decimal userMin) &&
                        decimal.TryParse(parts[1], out decimal userMax))
                    {
                        query = query.Where(j =>
                            j.MinSalary <= userMax &&
                            (j.MaxSalary ?? j.MinSalary) >= userMin
                        );
                    }
                }
            }

            query = filter.SortBy switch
            {
                "salary_desc" => query.OrderByDescending(j => j.MaxSalary ?? j.MinSalary),
                "salary_asc" => query.OrderBy(j => j.MinSalary ?? j.MaxSalary),
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
                    PostedDateDisplay = j.PostedDate.ToRelativeDate(),
                    SalaryDisplay = j.MinSalary.ToSalaryDisplay(j.MaxSalary),
                    DescriptionSnippet = j.Description.Length > 150
                        ? j.Description.Substring(0, 150) + "..."
                        : j.Description
                })
                .ToListAsync();

            foreach (var job in jobs)
                job.IsSaved = savedJobIds.Contains(job.Id);

            // Fetch available Industries
            var availableIndustries = IndustryMetadata.All()
                .Select(industry => new IndustryItemDto
                {
                    Icon = IndustryMetadata.GetIcon(industry),
                    DisplayName = IndustryMetadata.GetDisplayName(industry),
                    FilterValue = industry.ToString(),
                })
                .OrderBy(item => item.DisplayName)
                .ToList();

            // Fetch available Departments (No Job Counts Included)
            var availableDepartments = DepartmentMetadata.All()
                .Select(dept => new DepartmentItemDto
                {
                    Icon = DepartmentMetadata.GetIcon(dept),
                    DisplayName = DepartmentMetadata.GetDisplayName(dept),
                    FilterValue = dept.ToString()
                })
                .OrderBy(item => item.DisplayName)
                .ToList();

            return new JobBrowseResultDto
            {
                Jobs = jobs,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                Filter = filter,
                AvailableIndustries = availableIndustries,
                AvailableDepartments = availableDepartments
            };
        }

        public async Task<JobDetailDto?> GetJobDetailAsync(int jobId, string? candidateUserId = null)
        {
            await _jobLifecycleService.CloseExpiredJobsAsync();

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
                PostedDateDisplay = job.PostedDate.ToRelativeDate(),
                SalaryDisplay = job.MinSalary.ToSalaryDisplay(job.MaxSalary),
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
            questions ??= new List<JobQuestionDto>();

            var normalizedQuestions = questions
                .Where(q => !string.IsNullOrWhiteSpace(q.QuestionText))
                .Select((q, index) => new
                {
                    Question = q,
                    Order = index,
                    Type = Enum.TryParse<QuestionType>(q.Type, out var parsedType)
                        ? parsedType
                        : QuestionType.Essay
                })
                .ToList();

            var existing = await _context.JobQuestions
                .Where(q => q.JobId == jobId)
                .ToListAsync();

            var existingById = existing.ToDictionary(q => q.Id);
            var existingIds = existingById.Keys.ToList();

            var answeredQuestionIds = existingIds.Count == 0
                ? new HashSet<int>()
                : (await _context.ApplicationAnswers
                    .Where(a => existingIds.Contains(a.JobQuestionId))
                    .Select(a => a.JobQuestionId)
                    .Distinct()
                    .ToListAsync())
                    .ToHashSet();

            foreach (var item in normalizedQuestions)
            {
                var incoming = item.Question;

                if (incoming.Id.HasValue &&
                    existingById.TryGetValue(incoming.Id.Value, out var existingQuestion))
                {
                    if (answeredQuestionIds.Contains(existingQuestion.Id) &&
                        HasHistoricalQuestionChange(existingQuestion, incoming.QuestionText, item.Type))
                    {
                        existingQuestion.IsActive = false;

                        _context.JobQuestions.Add(new JobQuestion
                        {
                            JobId = jobId,
                            QuestionText = incoming.QuestionText.Trim(),
                            Type = item.Type,
                            IsActive = true,
                            IsRequired = incoming.IsRequired,
                            Order = item.Order
                        });

                        continue;
                    }

                    existingQuestion.QuestionText = incoming.QuestionText.Trim();
                    existingQuestion.Type = item.Type;
                    existingQuestion.IsActive = true;
                    existingQuestion.IsRequired = incoming.IsRequired;
                    existingQuestion.Order = item.Order;
                    continue;
                }

                _context.JobQuestions.Add(new JobQuestion
                {
                    JobId = jobId,
                    QuestionText = incoming.QuestionText.Trim(),
                    Type = item.Type,
                    IsActive = true,
                    IsRequired = incoming.IsRequired,
                    Order = item.Order
                });
            }

            var keptExistingIds = normalizedQuestions
                .Where(q => q.Question.Id.HasValue)
                .Select(q => q.Question.Id!.Value)
                .ToHashSet();

            foreach (var existingQuestion in existing.Where(q => q.IsActive && !keptExistingIds.Contains(q.Id)))
            {
                if (answeredQuestionIds.Contains(existingQuestion.Id))
                {
                    existingQuestion.IsActive = false;
                }
                else
                {
                    _context.JobQuestions.Remove(existingQuestion);
                }
            }

            await _context.SaveChangesAsync();
        }

        private static bool HasHistoricalQuestionChange(JobQuestion existingQuestion, string incomingQuestionText, QuestionType incomingType)
        {
            return !string.Equals(
                existingQuestion.QuestionText.Trim(),
                incomingQuestionText.Trim(),
                StringComparison.Ordinal) || existingQuestion.Type != incomingType;
        }

        private async Task<Job?> GetOwnedJobAsync(string userId, int jobId)
        {
            return await _context.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.Company.UserId == userId);
        }

        private static void EnsureDeadlineIsInFuture(DateTime applicationDeadline)
        {
            if (applicationDeadline <= DateTime.UtcNow)
                throw new ValidationException("Application deadline must be later than the current time.");
        }
    }
}