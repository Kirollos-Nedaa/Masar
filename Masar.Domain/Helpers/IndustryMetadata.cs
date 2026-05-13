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
            [Industries.Telecommunications] = ("lucide:radio-tower", "Telecom"),
            [Industries.MediaAndEntertainment] = ("lucide:clapperboard", "Media & Entertainment"),
            [Industries.Finance] = ("lucide:trending-up", "Finance"),
            [Industries.Insurance] = ("lucide:shield-check", "Insurance"),
            [Industries.RealEstate] = ("lucide:building", "Real Estate"),
            [Industries.Consulting] = ("lucide:users", "Consulting"),
            [Industries.Healthcare] = ("lucide:heart-pulse", "Healthcare"),
            [Industries.Pharmaceuticals] = ("lucide:flask-conical", "Pharmaceuticals"),
            [Industries.Retail] = ("lucide:shopping-bag", "Retail"),
            [Industries.Hospitality] = ("lucide:utensils", "Hospitality"),
            [Industries.ConsumerGoods] = ("lucide:package", "Consumer Goods"),
            [Industries.Manufacturing] = ("lucide:factory", "Manufacturing"),
            [Industries.Construction] = ("lucide:hard-hat", "Construction"),
            [Industries.Energy] = ("lucide:zap", "Energy"),
            [Industries.Transportation] = ("lucide:truck", "Transportation"),
            [Industries.Agriculture] = ("lucide:wheat", "Agriculture"),
            [Industries.Government] = ("lucide:landmark", "Government"),
            [Industries.Education] = ("lucide:graduation-cap", "Education"),
            [Industries.NonProfit] = ("lucide:heart-handshake", "Non-Profit"),
            [Industries.Other] = ("lucide:layout-grid", "Other"),
        };

        private const string FallbackIcon = "lucide:briefcase";

        // ── Public accessors ──────────────────────────────────────────────
        public static string GetIcon(Industries Industries) =>
            Map.TryGetValue(Industries, out var meta) ? meta.Icon : FallbackIcon;

        public static string GetDisplayName(Industries Industries) =>
            Map.TryGetValue(Industries, out var meta)
                ? meta.DisplayName
                : SplitPascalCase(Industries.ToString());

        public static IEnumerable<Industries> All() =>
            Enum.GetValues<Industries>().Where(i => i != Industries.None);

        // ── Fallback helper ───────────────────────────────────────────────
        private static string SplitPascalCase(string value) =>
            Regex.Replace(value, "([A-Z])", " $1").Trim();
    }
}