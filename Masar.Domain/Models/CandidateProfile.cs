using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.Models
{
    public class CandidateProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? ResumeUrl { get; set; }
        public int ProfileViews { get; set; } // Seen in Employee Dashboard UI

        public ICollection<Education> Educations { get; set; }
        public ICollection<CandidateSkill> CandidateSkills { get; set; }
        public ProfessionalLinks ProfessionalLinks { get; set; }
    }
}
