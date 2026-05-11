using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.HomeDtos
{
    public class FeaturedJobDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogo { get; set; }
        public string Location { get; set; } = string.Empty;
        public string JobTypeDisplay { get; set; } = string.Empty;
        public string? SalaryDisplay { get; set; }
        public string PostedDateDisplay { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
