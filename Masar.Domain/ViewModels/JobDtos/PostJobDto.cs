using Masar.Domain.Enums;
using Masar.Domain.ViewModels.JobDtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.Job
{
    public class PostJobDto
    {
        // ── Basic Information ─────────────────────────────────
        [Required(ErrorMessage = "Job title is required.")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job type is required.")]
        public JobType JobType { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        public Department Department { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Work mode is required.")]
        public WorkMode WorkMode { get; set; }

        // ── Compensation ──────────────────────────────────────
        [Range(0, double.MaxValue, ErrorMessage = "Minimum salary must be a positive number.")]
        public decimal? MinSalary { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Maximum salary must be a positive number.")]
        public decimal? MaxSalary { get; set; }

        // ── Job Description ───────────────────────────────────
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(4000)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Requirements are required.")]
        [StringLength(4000)]
        public string Requirements { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? Benefits { get; set; }

        // ── Application Details ───────────────────────────────
        [Required(ErrorMessage = "Application deadline is required.")]
        public DateTime ApplicationDeadline { get; set; } = DateTime.UtcNow.AddDays(30);

        [Required(ErrorMessage = "Number of openings is required.")]
        [Range(1, 1000, ErrorMessage = "Number of openings must be between 1 and 1000.")]
        public int NumberOfOpenings { get; set; } = 1;

        public bool RequireCv { get; set; } = true;
        public bool RequireCoverLetter { get; set; }

        // ── Application Questions ─────────────────────────────
        public List<JobQuestionDto> Questions { get; set; } = new();
    }
}