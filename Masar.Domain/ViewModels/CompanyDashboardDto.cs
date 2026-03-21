using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels
{
    public class CompanyDashboardDto
    {
        public int ActiveJobs { get; set; }
        public int TotalApplicants { get; set; }
        public int NewApplicants { get; set; }
        public int PendingReviews { get; set; }
        public List<PostedJobDto> PostedJobs { get; set; } = new();
        public List<RecentApplicantDto> RecentApplicants { get; set; } = new();
    }
}
