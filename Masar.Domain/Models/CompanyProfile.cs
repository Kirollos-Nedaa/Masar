using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.Models
{
    public class CompanyProfile
    {
        public int Id { get; set; }

        // Navigational property to ApplicationUser
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string CompanyName { get; set; }
        public string CompanyLogo { get; set; }
        public string Description { get; set; }
    }
}
