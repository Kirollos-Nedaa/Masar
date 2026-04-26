using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.JobDtos
{
    public class JobAnswerDto
    {
        public int QuestionId { get; set; }
        public string? AnswerText { get; set; }
    }
}
