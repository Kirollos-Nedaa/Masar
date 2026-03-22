using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.Enums
{
    public enum CompanySize
    {
        None = 0,
        Mini = 1,               // 1-10 employees
        Small = 2,              // 11-50 employees
        Medium = 3,             // 51-250 employees
        Large = 4,              // 251-1000 employees
        Enterprise = 5          // 1001+ employees
    }
}
