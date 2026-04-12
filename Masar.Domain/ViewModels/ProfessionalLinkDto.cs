using Masar.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels
{
    public class ProfessionalLinkDto
    {
        public int? Id { get; set; }
        public LinksNames LinkName { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}
