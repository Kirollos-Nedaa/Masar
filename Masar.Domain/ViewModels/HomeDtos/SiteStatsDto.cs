using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.HomeDtos
{
    public class SiteStatsDto
    {
        public string TotalJobsDisplay { get; set; } = "0";
        public string TotalCompaniesDisplay { get; set; } = "0";
        public string TotalCandidatesDisplay { get; set; } = "0";
        public string HiringRateDisplay { get; set; } = "0%";
    }
}
