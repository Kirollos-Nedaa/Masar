using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.JobDtos
{
    public class JobFilterDto
    {
        public string? Search { get; set; }
        public string? Location { get; set; }
        public List<string> JobTypes { get; set; } = new();
        public List<string> Industries { get; set; } = new();
        public string? SalaryRange { get; set; }
        public string SortBy { get; set; } = "recent";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 6;
    }
}
