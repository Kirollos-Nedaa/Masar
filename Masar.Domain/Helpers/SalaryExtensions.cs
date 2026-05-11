using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.Helpers
{
    public static class SalaryExtensions
    {
        public static string ToSalaryDisplay(this decimal? minSalary, decimal? maxSalary)
        {
            if (minSalary.HasValue && maxSalary.HasValue)
            {
                // UI/UX comfort: If min and max are identical, just show a single number
                if (minSalary.Value == maxSalary.Value)
                    return $"${FormatCompactNumber(minSalary.Value)}";

                return $"${FormatCompactNumber(minSalary.Value)} – ${FormatCompactNumber(maxSalary.Value)}";
            }

            if (minSalary.HasValue)
                return $"Starting from ${FormatCompactNumber(minSalary.Value)}";

            if (maxSalary.HasValue)
                return $"Up to ${FormatCompactNumber(maxSalary.Value)}";

            return null;
        }
        private static string FormatCompactNumber(decimal number)
        {
            if (number < 1000)
            {
                // "0" removes trailing decimal zeros for numbers under 1,000 (e.g., 500.00 becomes 500)
                return number.ToString("0");
            }

            if (number < 1000000)
            {
                // Divides by 1,000 and adds 'K'. "0.#" handles 1.5K but keeps 1K clean.
                return (number / 1000m).ToString("0.#") + "K";
            }

            // Handles millions
            return (number / 1000000m).ToString("0.#") + "M";
        }
    }
}
