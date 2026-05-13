using Masar.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Masar.Domain.Helpers
{
    public static class JobTypeMetadata
    {
        private static readonly Dictionary<JobType, (string Icon, string DisplayName)> Map = new()
        {
            [JobType.FullTime] = ("lucide:briefcase", "Full-Time"),
            [JobType.PartTime] = ("lucide:clock", "Part-Time"),
            [JobType.Internship] = ("lucide:graduation-cap", "Internship")
        };

        private const string FallbackIcon = "lucide:briefcase";

        public static string GetIcon(JobType jt) =>
            Map.TryGetValue(jt, out var meta) ? meta.Icon : FallbackIcon;

        public static string GetDisplayName(JobType jt) =>
            Map.TryGetValue(jt, out var meta)
                ? meta.DisplayName
                : Regex.Replace(jt.ToString(), "([A-Z])", " $1").Trim();

        public static IEnumerable<JobType> All() =>
            Enum.GetValues<JobType>().Where(j => j != JobType.None);
    }
}
