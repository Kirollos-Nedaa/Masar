using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.HomeDtos
{
    public class HomePageDto
    {
        public List<LatestOpeningDto> LatestOpenings { get; set; } = new();
        public SiteStatsDto SiteStats { get; set; } = new();
        public List<IndustryItemDto> Industries { get; set; } = new();
        public List<FeaturedJobDto> FeaturedJobs { get; set; } = new();
    }
}
