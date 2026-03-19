using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.Models
{
    public class Education
    {
        public int Id { get; set; }

        public int CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; }

        public string University { get; set; }
        public string Degree { get; set; }
        public string Major { get; set; }
        public int StartYear { get; set; }
        public int ExpectedGraduation { get; set; }
    }
}
