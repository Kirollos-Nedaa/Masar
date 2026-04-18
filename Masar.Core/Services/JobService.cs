using Masar.Core.IService;
using Masar.Domain.Models;
using Masar.Domain.ViewModels;
using Masar.Domain.ViewModels.CompanyDtos;
using Masar.Domain.ViewModels.Job;
using Masar.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Masar.Core.Services
{
    public class JobService : IJobService
    {
        private readonly AppDbContext _context;

        public JobService(AppDbContext context)
        {
            _context = context;
        }

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
            var job = await GetOwnedJobAsync(userId, jobId);
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
                RequireCoverLetter = job.RequireCoverLetter
            };
        }

        public async Task<List<JobListItemDto>> GetCompanyJobsAsync(string userId)
        {
            var company = await _context.CompanyProfiles
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (company == null) return new List<JobListItemDto>();

            return await _context.Jobs
                .Where(j => j.CompanyProfileId == company.Id)
                .OrderByDescending(j => j.PostedDate)
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
                    PostedDateDisplay = j.PostedDate <= DateTime.UtcNow.AddDays(-30)
                        ? j.PostedDate.ToString("MMM dd, yyyy")
                        : j.PostedDate > DateTime.UtcNow.AddDays(-1) ? "Today"
                        : j.PostedDate > DateTime.UtcNow.AddDays(-2) ? "Yesterday"
                        : (int)(DateTime.UtcNow - j.PostedDate).TotalDays + " days ago"
                })
                .ToListAsync();
        }

        private async Task<Job?> GetOwnedJobAsync(string userId, int jobId)
        {
            return await _context.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.Company.UserId == userId);
        }
    }
}