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
    public class SavedJobConfiguration : IEntityTypeConfiguration<SavedJob>
    {
        public void Configure(EntityTypeBuilder<SavedJob> builder)
        {
            builder.HasKey(sj => sj.Id);

            // Prevent a candidate from saving the same job twice
            builder.HasIndex(sj => new { sj.CandidateProfileId, sj.JobId })
                .IsUnique();

            builder.Property(sj => sj.SavedAt)
                .IsRequired();

            builder.HasOne(sj => sj.Job)
                .WithMany(j => j.SavedJobs)
                .HasForeignKey(sj => sj.JobId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(sj => sj.Candidate)
                .WithMany(c => c.SavedJobs)
                .HasForeignKey(sj => sj.CandidateProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            //------------------------------------------
            builder.HasIndex(sj => sj.CandidateProfileId);
            builder.HasIndex(sj => sj.JobId);
        }
    }
}
