using Masar.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Masar.Domain.Helpers
{
    public static class DepartmentMetadata
    {
        private static readonly Dictionary<Department, (string Icon, string DisplayName)> Map = new()
        {
            [Department.SoftwareEngineering] = ("lucide:code", "Software Engineering"),
            [Department.InformationTechnology] = ("lucide:server", "Information Technology"),
            [Department.DataAndAnalytics] = ("lucide:database", "Data & Analytics"),
            [Department.QualityAssurance] = ("lucide:bug", "Quality Assurance"),
            [Department.HardwareEngineering] = ("lucide:microchip", "Hardware Engineering"),
            [Department.ProductManagement] = ("lucide:kanban", "Product Management"),
            [Department.DesignAndUX] = ("lucide:pen-tool", "Design & UX"),
            [Department.ResearchAndDevelopment] = ("lucide:lightbulb", "R&D"),
            [Department.Sales] = ("lucide:badge-dollar-sign", "Sales"),
            [Department.Marketing] = ("lucide:megaphone", "Marketing"),
            [Department.CustomerService] = ("lucide:headset", "Customer Service"),
            [Department.AccountManagement] = ("lucide:contact", "Account Management"),
            [Department.HumanResources] = ("lucide:users", "Human Resources"),
            [Department.Finance] = ("lucide:calculator", "Finance"),
            [Department.Operations] = ("lucide:settings", "Operations"),
            [Department.Legal] = ("lucide:scale", "Legal & Compliance"),
            [Department.Administration] = ("lucide:clipboard-list", "Administration"),
            [Department.Executive] = ("lucide:crown", "Executive Leadership"),
            [Department.Clinical] = ("lucide:stethoscope", "Medical & Clinical"),
            [Department.Production] = ("lucide:boxes", "Manufacturing & Production"),
            [Department.Other] = ("lucide:briefcase", "Other")
        };

        private const string FallbackIcon = "lucide:users";

        public static string GetIcon(Department department) =>
            Map.TryGetValue(department, out var meta) ? meta.Icon : FallbackIcon;

        public static string GetDisplayName(Department department) =>
            Map.TryGetValue(department, out var meta)
                ? meta.DisplayName
                : Regex.Replace(department.ToString(), "([A-Z])", " $1").Trim();

        public static IEnumerable<Department> All() =>
            Enum.GetValues<Department>().Where(d => d != Department.None);
    }
}
