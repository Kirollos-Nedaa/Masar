using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.JobDtos
{
    public class JobQuestionDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Question text is required.")]
        [StringLength(500)]
        public string QuestionText { get; set; } = string.Empty;

        // "Essay" or "TorF"
        public string Type { get; set; } = "Essay";
        public bool IsRequired { get; set; } = true;
        public int Order { get; set; }
    }
}
