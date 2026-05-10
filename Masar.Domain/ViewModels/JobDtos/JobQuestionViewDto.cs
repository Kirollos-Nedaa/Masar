using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.JobDtos
{
    public class JobQuestionViewDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Essay", "TorF"
        public bool IsRequired { get; set; }
        public int Order { get; set; }
    }
}
