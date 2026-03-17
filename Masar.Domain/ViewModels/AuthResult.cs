using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? UserId { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}
