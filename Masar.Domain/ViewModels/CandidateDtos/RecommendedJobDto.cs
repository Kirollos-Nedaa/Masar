using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CandidateDtos
{
    public class RecommendedJobDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public string PostedDate { get; set; }
        public string? Salary { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
    }
}
