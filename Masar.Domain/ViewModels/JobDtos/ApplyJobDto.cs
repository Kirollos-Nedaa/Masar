using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.JobDtos
{
    public class ApplyJobDto
    {
        public int JobId { get; set; }

        // ── Personal Info (pre-filled, editable) ──────────────
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }

        // ── Resume ────────────────────────────────────────────
        public bool UseExistingResume { get; set; } = true;
        public string? ExistingResumeUrl { get; set; }
        // File upload handled separately in controller via IFormFile

        // ── Cover Letter ──────────────────────────────────────
        public string? CoverLetter { get; set; }

        // ── Terms ─────────────────────────────────────────────
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the terms and conditions.")]
        public bool AgreeToTerms { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must consent to data processing.")]
        public bool ConsentToDataProcessing { get; set; }
    }
}
