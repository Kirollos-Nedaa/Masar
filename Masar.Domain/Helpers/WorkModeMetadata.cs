using Masar.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Masar.Domain.Helpers
{
    public static class WorkModeMetadata
    {
        private static readonly Dictionary<WorkMode, (string Icon, string DisplayName)> Map = new()
        {
            [WorkMode.OnSite] = ("lucide:building-2", "On-Site"),
            [WorkMode.Remote] = ("lucide:home", "Remote"),
            [WorkMode.Hybrid] = ("lucide:git-merge", "Hybrid")
        };

        private const string FallbackIcon = "lucide:map-pin";

        public static string GetIcon(WorkMode wm) =>
            Map.TryGetValue(wm, out var meta) ? meta.Icon : FallbackIcon;

        public static string GetDisplayName(WorkMode wm) =>
            Map.TryGetValue(wm, out var meta)
                ? meta.DisplayName
                : Regex.Replace(wm.ToString(), "([A-Z])", " $1").Trim();

        public static IEnumerable<WorkMode> All() =>
            Enum.GetValues<WorkMode>().Where(w => w != WorkMode.None);
    }
}
