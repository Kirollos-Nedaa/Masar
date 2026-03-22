using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Masar.Domain.Enums;

namespace Masar.Domain.Models
{
    public class ProfessionalLink
    {
        public int Id { get; set; }

        // Navigational property to CandidateProfile & CompanyProfile
        public int? CandidateProfileId { get; set; }
        public CandidateProfile? CandidateProfile { get; set; }
        public int? CompanyProfileId { get; set; }
        public CompanyProfile? CompanyProfile { get; set; }

        public LinksNames LinksNames { get; set; }
        public string Url { get; set; }
    }
}
