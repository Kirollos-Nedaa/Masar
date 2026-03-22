using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels
{
    public class RecentApplicationDto
    {
        public string JobTitle { get; set; }
        public string Company { get; set; }
        public string AppliedDate { get; set; }
        public string Status { get; set; }
    }
}
