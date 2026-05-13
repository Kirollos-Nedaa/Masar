using Masar.Core.IService;
using Masar.Domain.Enums;
using Masar.Domain.Helpers;
using Masar.Domain.ViewModels;
using Masar.Domain.ViewModels.HomeDtos;
using Masar.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Masar.Core.Services
{
    public class HomeService : IHomeService
    {
        private readonly AppDbContext _context;
        private readonly IJobLifecycleService _jobLifecycleService;

        public HomeService(AppDbContext context, IJobLifecycleService jobLifecycleService)
        {
            _context = context;
            _jobLifecycleService = jobLifecycleService;
        }

        public async Task<HomePageDto> GetHomePageDataAsync()
        {
            // Close any expired jobs so counts are accurate
            await _jobLifecycleService.CloseExpiredJobsAsync();

            // shifts the time to get the latest 24h
            var cutoffTime = DateTime.UtcNow.AddHours(-24);

            // ── 1. Latest 6 active jobs ──────────────────────────────
            var latestOpenings = await _context.Jobs
                .Include(j => j.Company)
                .Where(j => j.IsActive && j.PostedDate >= cutoffTime)
                .OrderByDescending(j => j.PostedDate)
                .Take(6)
                .Select(j => new LatestOpeningDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    CompanyName = j.Company.Name ?? string.Empty,
                    CompanyLogo = j.Company.LogoUrl,
                    SalaryDisplay = j.MinSalary.ToSalaryDisplay(j.MaxSalary),
                    JobTypeDisplay = j.JobType == JobType.FullTime ? "Full-time"
                                   : j.JobType == JobType.PartTime ? "Part-time"
                                   : j.JobType == JobType.Internship ? "Internship"
                                   : j.JobType.ToString()
                })
                .ToListAsync();

            // ── 2. Site-wide stats ───────────────────────────────────
            var totalActiveJobs = await _context.Jobs.CountAsync(j => j.IsActive);
            var totalCompanies = await _context.CompanyProfiles.CountAsync();
            var totalCandidates = await _context.CandidateProfiles.CountAsync();
            var totalApps = await _context.JobApplications.CountAsync();
            var acceptedApps = await _context.JobApplications.CountAsync(a => a.Status == ApplicationStatus.Accepted);

            int hiringRate = totalApps > 0
                ? (int)Math.Round((double)acceptedApps / totalApps * 100)
                : 0;

            var stats = new SiteStatsDto
            {
                TotalJobsDisplay = FormatStat(totalActiveJobs),
                TotalCompaniesDisplay = FormatStat(totalCompanies),
                TotalCandidatesDisplay = FormatStat(totalCandidates),
                HiringRateDisplay = $"{hiringRate}%"
            };

            // ── 3.1 Industry company counts (single DB round-trip) ─────
            var industryCompanyCount = await _context.Jobs
                .Where(j => j.IsActive && j.Company.Industry != null)
                .GroupBy(j => j.Company.Industry)
                .Select(g => new { 
                    Industry = g.Key,
                    CompanyCount = g.Select(j => j.Company.Id).Distinct().Count()
                })
                .ToListAsync();

            // ── 3.2 Department job counts (single DB round-trip) ─────
            var departmentJobsCount = await _context.Jobs
                .Where(j => j.IsActive && j.Department != null)
                .GroupBy(j => j.Department)
                .Select(g => new
                {
                    Department = g.Key,
                    JobCount = g.Count()
                })
                .ToListAsync();

            var countLookup = industryCompanyCount
                .Where(x => x.Industry != null)
                .ToDictionary(x => x.Industry!, x => x.CompanyCount);

            var deptCountLookup = departmentJobsCount
                .Where(x => x.Department != null)
                .ToDictionary(x => x.Department!, x => x.JobCount);

            // ── 4. Build industry & Department list from enum — fully automatic ─
            var industries = IndustryMetadata.All()
                .Select(industry =>
                {
                    var filterValue = industry.ToString();
                    return new IndustryItemDto
                    {
                        Icon = IndustryMetadata.GetIcon(industry),
                        DisplayName = IndustryMetadata.GetDisplayName(industry),
                        FilterValue = filterValue,
                        CompanyCount = countLookup.GetValueOrDefault(filterValue, 0)
                    };
                })
                .OrderBy(item => item.DisplayName)
                .ToList();

            var departments = DepartmentMetadata.All()
                .Select(dept =>
                {
                    var filterValue = dept.ToString();
                    return new DepartmentItemDto
                    {
                        Icon = DepartmentMetadata.GetIcon(dept),
                        DisplayName = DepartmentMetadata.GetDisplayName(dept),
                        FilterValue = filterValue,
                        JobCount = deptCountLookup.GetValueOrDefault(dept, 0)
                    };
                })
                .OrderBy(item => item.DisplayName)
                .ToList();

            // ── 5. Featured / latest 6 active jobs ──────────────────
            var featuredJobs = await _context.Jobs
                .Include(j => j.Company)
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.IsFeatured)
                .ThenByDescending(j => j.PostedDate)
                .Take(6)
                .Select(j => new FeaturedJobDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    CompanyName = j.Company.Name ?? string.Empty,
                    CompanyLogo = j.Company.LogoUrl,
                    Location = j.Location,
                    JobTypeDisplay = j.JobType == JobType.FullTime ? "Full-time"
                                    : j.JobType == JobType.PartTime ? "Part-time"
                                    : j.JobType == JobType.Internship ? "Internship"
                                    : j.JobType.ToString(),
                    SalaryDisplay = j.MinSalary.ToSalaryDisplay(j.MaxSalary),
                    PostedDateDisplay = j.PostedDate.ToRelativeDate(),
                    Description = j.Description.Length > 276
                                        ? j.Description.Substring(0, 276) + "..."
                                        : j.Description
                })
                .ToListAsync();

            return new HomePageDto
            {
                LatestOpenings = latestOpenings,
                SiteStats = stats,
                Industries = industries,
                Departments = departments,
                FeaturedJobs = featuredJobs
            };
        }

        // ── Helpers ───────────────────────────────────────────────
        private static string FormatStat(long number)
        {
            if (number < 1_000)
                return number.ToString();

            if (number < 1_000_000)
            {
                double k = number / 1_000.0;
                double truncated = Math.Floor(k * 10) / 10.0;   // 1 decimal, truncated

                // If decimal part is zero, skip it (e.g. 2.0K+ → "2K+")
                return truncated == Math.Floor(truncated)
                    ? $"{(int)truncated}K+"
                    : $"{truncated:0.0}K+";
            }

            // Millions
            double m = number / 1_000_000.0;
            double mTrunc = Math.Floor(m * 10) / 10.0;
            return mTrunc == Math.Floor(mTrunc) ? $"{(int)mTrunc}M+" : $"{mTrunc:0.0}M+";
        }
    }
}
