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
        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }

        // ── Resume ────────────────────────────────────────────
        public bool UseExistingResume { get; set; } = true;
        public string? ExistingResumeUrl { get; set; }

        // ── Cover Letter ──────────────────────────────────────
        public string CoverLetter { get; set; } = string.Empty;

        // ── Terms ─────────────────────────────────────────────
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the terms and conditions.")]
        public bool AgreeToTerms { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must consent to data processing.")]
        public bool ConsentToDataProcessing { get; set; }

        public List<JobAnswerDto> Answers { get; set; } = [];
    }
}
