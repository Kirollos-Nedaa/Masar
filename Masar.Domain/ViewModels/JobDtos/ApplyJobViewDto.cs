using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.JobDtos
{
    public class ApplyJobViewDto
    {
        // ── Job context ───────────────────────────────────────
        public JobDetailDto Job { get; set; } = new();

        // ── Form data ─────────────────────────────────────────
        public ApplyJobDto Form { get; set; } = new();

        // ── Job questions for this posting ────────────────────
        public List<JobQuestionViewDto> Questions { get; set; } = new();
    }
}
