using Masar.Domain.Enums;
using System.Text.RegularExpressions;

namespace Masar.Domain.Helpers
{
    public static class CompanySizeMetadata
    {
        // ── Explicit mappings ─────────────────────────────────────────────
        private static readonly Dictionary<CompanySize, (string Icon, string DisplayName)> Map = new()
        {
            [CompanySize.Mini] = ("lucide:user", "1–10 employees"),
            [CompanySize.Small] = ("lucide:users", "11–50 employees"),
            [CompanySize.Medium] = ("lucide:users", "51–250 employees"),
            [CompanySize.Large] = ("lucide:building", "251–1,000 employees"),
            [CompanySize.Enterprise] = ("lucide:building-2", "1,001+ employees")
        };

        private const string FallbackIcon = "lucide:users";

        // ── Public accessors ──────────────────────────────────────────────
        public static string GetIcon(CompanySize size) =>
            Map.TryGetValue(size, out var meta) ? meta.Icon : FallbackIcon;

        public static string GetDisplayName(CompanySize size) =>
            Map.TryGetValue(size, out var meta)
                ? meta.DisplayName
                : SplitPascalCase(size.ToString());

        public static IEnumerable<CompanySize> All() => Enum.GetValues<CompanySize>().Where(s => s != CompanySize.None);

        // ── Fallback helper ───────────────────────────────────────────────
        private static string SplitPascalCase(string value) => Regex.Replace(value, "([A-Z])", " $1").Trim();
    }
}
