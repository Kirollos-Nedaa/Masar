using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CompanyDtos
{
    public class ReviewAnswerDto
    {
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
