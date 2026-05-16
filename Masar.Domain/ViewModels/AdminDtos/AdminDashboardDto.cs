namespace Masar.Domain.ViewModels.AdminDtos
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalCandidates { get; set; }
        public int TotalCompanies { get; set; }
        public int ActiveJobs { get; set; }
        public int TotalApplications { get; set; }

        public List<AdminRecentUserDto> RecentUsers { get; set; } = new();
        public List<AdminRecentJobDto> RecentPosts { get; set; } = new();
    }
}
