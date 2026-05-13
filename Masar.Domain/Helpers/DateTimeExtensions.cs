using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.Helpers
{
    public static class DateTimeExtensions
    {
        public static string ToRelativeDate(this DateTime date)
        {
            var diff = DateTime.UtcNow - date;

            if (diff.TotalMinutes < 1)
                return "Just now";

            if (diff.TotalMinutes < 60)
            {
                int minutes = (int)diff.TotalMinutes;
                return $"{minutes} {(minutes > 1 ? "minute" : "minutes")} ago";
            }

            if (diff.TotalHours < 24)
            {
                int hours = (int)diff.TotalHours;
                return $"{hours} {(hours > 1 ? "hour" : "hours")} ago";
            }

            if (diff.TotalDays < 7)
            {
                int days = (int)diff.TotalDays;
                if (days == 1) return "Yesterday";
                return $"{days} days ago";
            }

            if (diff.TotalDays < 30)
            {
                int weeks = (int)(diff.TotalDays / 7);
                return $"{weeks} {(weeks > 1 ? "week" : "weeks")} ago";
            }

            return date.ToString("dd/MM/yyyy");
        }
    }
}
