using Masar.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.Models
{
    public class CompanyProfile
    {
        public int Id { get; set; }

        // Navigational property to ApplicationUser
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string? Name { get; set; }
        public string? LogoUrl { get; set; }
        public string? Industry { get; set; }
        public CompanySize? Size { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public CompanyContactInfo? ContactInfo { get; set; }
        public ICollection<ProfessionalLink> ProfessionalLinks { get; set; } = new List<ProfessionalLink>();
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
