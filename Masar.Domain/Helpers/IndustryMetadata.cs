using System.Text.RegularExpressions;
using Masar.Domain.Enums;

namespace Masar.Domain.Helpers
{
    public static class IndustryMetadata
    {
        // ── Explicit mappings ─────────────────────────────────────────────
        private static readonly Dictionary<Industries, (string Icon, string DisplayName)> Map = new()
        {
            [Industries.Technology] = ("lucide:cpu", "Technology"),
            [Industries.Finance] = ("lucide:trending-up", "Finance"),
            [Industries.Healthcare] = ("lucide:heart-pulse", "Healthcare"),
            [Industries.Education] = ("lucide:graduation-cap", "Education"),
            [Industries.Manufacturing] = ("lucide:factory", "Manufacturing"),
            [Industries.Retail] = ("lucide:shopping-bag", "Retail"),
            [Industries.Energy] = ("lucide:zap", "Energy"),
            [Industries.Transportation] = ("lucide:truck", "Transportation"),
            [Industries.Hospitality] = ("lucide:utensils", "Hospitality"),
            [Industries.Construction] = ("lucide:hard-hat", "Construction"),
            [Industries.Agriculture] = ("lucide:wheat", "Agriculture"),
            [Industries.Media] = ("lucide:tv-2", "Media"),
            [Industries.Telecommunications] = ("lucide:radio-tower", "Telecom"),
            [Industries.Government] = ("lucide:landmark", "Government"),
            [Industries.NonProfit] = ("lucide:heart-handshake", "Non-Profit"),
            [Industries.Operations] = ("lucide:settings", "Operations"),
            [Industries.ArtAndDesign] = ("lucide:palette", "Art & Design"),
            [Industries.Other] = ("lucide:briefcase", "Other"),
        };

        private const string FallbackIcon = "lucide:briefcase";

        // ── Public accessors ──────────────────────────────────────────────
        public static string GetIcon(Industries industry) => Map.TryGetValue(industry, out var meta) ? meta.Icon : FallbackIcon;
        public static string GetDisplayName(Industries industry) => Map.TryGetValue(industry, out var meta)
            ? meta.DisplayName
            : SplitPascalCase(industry.ToString());
        public static IEnumerable<Industries> All() => Enum.GetValues<Industries>().Where(i => i != Industries.None);

        // ── Fallback helper ───────────────────────────────────────────────
        private static string SplitPascalCase(string value) => Regex.Replace(value, "([A-Z])", " $1").Trim();
    }
}