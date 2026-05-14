using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.AdminDtos
{
    public class AdminRecentJobDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public string Status { get; set; }
        public string PostedDate { get; set; }
    }
}
