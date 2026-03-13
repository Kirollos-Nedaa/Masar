using Masar.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Config
{
    public class CandidateSkillConfiguration : IEntityTypeConfiguration<CandidateSkill>
    {
        public void Configure(EntityTypeBuilder<CandidateSkill> builder)
        {
            builder.HasKey(cs => cs.Id);

            // Prevent a candidate from having the same skill twice
            builder.HasIndex(cs => new { cs.CandidateProfileId, cs.SkillId })
                .IsUnique();

            builder.Property(cs => cs.CandidateProfileId)
                .IsRequired();

            builder.Property(cs => cs.SkillId)
                .IsRequired();
        }
    }
}
