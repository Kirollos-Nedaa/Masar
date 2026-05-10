using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.Job
{
    public class JobListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string WorkMode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int ApplicantCount { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime ApplicationDeadline { get; set; }
        public string PostedDateDisplay { get; set; } = string.Empty;
        public bool IsExpired => ApplicationDeadline < DateTime.UtcNow;
    }
}