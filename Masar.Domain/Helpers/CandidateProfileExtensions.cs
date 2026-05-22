using Masar.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.Helpers
{
    public static class CandidateProfileExtensions
    {
        public static int CalculateProfileCompletion(this CandidateProfile profile, ApplicationUser user)
        {
            if (profile == null || user == null) return 0;

            int score = 0;

            // Personal info — 30 pts
            bool hasAllPersonalInfo =
                !string.IsNullOrWhiteSpace(user.FirstName) &&
                !string.IsNullOrWhiteSpace(user.LastName) &&
                !string.IsNullOrWhiteSpace(user.Email) &&
                profile.Gender.HasValue &&
                !string.IsNullOrWhiteSpace(profile.Location) &&
                profile.DateOfBirth.HasValue &&
                !string.IsNullOrWhiteSpace(profile.PhoneNumber) &&
                !string.IsNullOrWhiteSpace(profile.Bio);

            if (hasAllPersonalInfo)
                score += 30;

            // Education — 30 pts
            if (profile.Educations?.Any() == true)
                score += 30;

            // Skills — 20 pts
            if (profile.CandidateSkills?.Any() == true)
                score += 20;

            // Professional links — 20 pts
            if (profile.ProfessionalLinks?.Any() == true)
                score += 20;

            return score;
        }

        public static List<string> GetProfileCompletionHints(this CandidateProfile profile, ApplicationUser user)
        {
            var hints = new List<string>();
            if (profile == null || user == null) return hints;

            // Personal Info (ALL required)
            bool hasAllPersonalInfo =
                !string.IsNullOrWhiteSpace(user.FirstName) &&
                !string.IsNullOrWhiteSpace(user.LastName) &&
                !string.IsNullOrWhiteSpace(user.Email) &&
                profile.Gender.HasValue &&
                !string.IsNullOrWhiteSpace(profile.Location) &&
                profile.DateOfBirth.HasValue &&
                !string.IsNullOrWhiteSpace(profile.PhoneNumber) &&
                !string.IsNullOrWhiteSpace(profile.Bio);

            if (!hasAllPersonalInfo)
                hints.Add("Complete your personal information");

            // Education
            if (profile.Educations?.Any() != true)
                hints.Add("Add your education");

            // Skills
            if (profile.CandidateSkills?.Any() != true)
                hints.Add("Add your skills");

            // Professional Links
            if (profile.ProfessionalLinks?.Any() != true)
                hints.Add("Add professional links");

            return hints;
        }

        // NEW: A clean boolean check for the Job Application gate
        public static bool IsProfileComplete(this CandidateProfile profile, ApplicationUser user)
        {
            return profile.CalculateProfileCompletion(user) == 100;
        }
    }
}
