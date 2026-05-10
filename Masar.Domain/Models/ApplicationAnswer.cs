using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.Models
{
    public class ApplicationAnswer
    {
        public int Id { get; set; }

        public int JobApplicationId { get; set; }
        public JobApplication JobApplication { get; set; } = null!;

        public int JobQuestionId { get; set; }
        public JobQuestion JobQuestion { get; set; } = null!;

        public string AnswerText { get; set; } = string.Empty;
    }
}
