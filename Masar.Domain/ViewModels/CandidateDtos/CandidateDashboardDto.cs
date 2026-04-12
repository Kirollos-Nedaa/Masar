using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CandidateDtos
{
    public class CandidateDashboardDto
    {
        public string Name { get; set; }
        public int ProfileCompletion { get; set; }
        public List<string> ProfileHints { get; set; }
        public int TotalApplications { get; set; }
        public int SavedJobs { get; set; }
        public int UnderReview { get; set; }
        public List<RecentApplicationDto> RecentApplications { get; set; } = new();
        public List<RecommendedJobDto> RecommendedJobs { get; set; } = new();
    }
}
