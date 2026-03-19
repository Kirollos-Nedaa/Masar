using Masar.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.Models
{
    public class JobQuestion
    {
        public int Id { get; set; }

        public int JobId { get; set; }
        public Job Job { get; set; }

        public string QuestionText { get; set; }
        public QuestionType Type { get; set; }
        public bool IsRequired { get; set; }
        public int Order { get; set; }
    }
}
