using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CompanyDtos
{
    public class RecentApplicantDto
    {
        public int JobId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string AppliedDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
