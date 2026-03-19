using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.Models
{
    public class CompanyContactInfo
    {
        public int Id { get; set; }

        // Navigational property to ApplicationUser
        public int CompanyProfileId { get; set; }
        public CompanyProfile CompanyProfile { get; set; }

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}
