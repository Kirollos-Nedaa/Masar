using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.Models
{
    public class SavedJob
    {
        public int Id { get; set; }

        public int CandidateProfileId { get; set; }
        public CandidateProfile Candidate { get; set; }
        public int JobId { get; set; }
        public Job Job { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}
