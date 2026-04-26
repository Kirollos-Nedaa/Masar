using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CompanyDtos
{
    public class ReviewEducationDto
    {
        public string University { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public string Years { get; set; } = string.Empty;
    }
}
