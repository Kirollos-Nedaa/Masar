using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels
{
    public class IndustryItemDto
    {
        public string Icon { get; set; } = "lucide:briefcase";
        public string DisplayName { get; set; } = string.Empty;
        public string FilterValue { get; set; } = string.Empty;
        public int CompanyCount { get; set; }
    }
}
