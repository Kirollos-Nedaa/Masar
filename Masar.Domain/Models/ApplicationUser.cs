using Masar.Domain.Enums;
using Masar.Domain.Helpers;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; } = AppTime.Now;

        public CandidateProfile? CandidateProfile { get; set; }
        public CompanyProfile? CompanyProfile { get; set; }
    }
}
