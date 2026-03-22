using Masar.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.Models
{
    public class Job
    {
        public int Id { get; set; }

        // Foreign key to CompanyProfile
        public int CompanyProfileId { get; set; }
        public CompanyProfile Company { get; set; }

        public string Title { get; set; }
        public JobType JobType { get; set; }
        public Department Department { get; set; }
        public string Location { get; set; }
        public WorkMode WorkMode { get; set; }

        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }

        public string Description { get; set; }
        public string Requirements { get; set; }
        public string? Benefits { get; set; }

        public DateTime ApplicationDeadline { get; set; }
        public int NumberOfOpenings { get; set; }
        public bool RequireCv { get; set; }
        public bool RequireCoverLetter { get; set; }

        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime PostedDate { get; set; } = DateTime.UtcNow;

        public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
        public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
        public ICollection<JobQuestion> JobQuestions { get; set; } = new List<JobQuestion>();
    }
}
