using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Domain.ViewModels.CandidateDtos
{
    public class EditSkillsDto
    {
        public List<SkillItemDto> CurrentSkills { get; set; } = new();
        public List<SkillItemDto> AvailableSkills { get; set; } = new();
        public List<int> SelectedSkillIds { get; set; } = new();
        public string? CustomSkillName { get; set; }

        public const int MinSkills = 0;
        public const int MaxSkills = 20;
    }
}
