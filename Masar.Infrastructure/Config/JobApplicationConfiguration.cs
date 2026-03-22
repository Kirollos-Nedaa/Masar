using Masar.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Masar.Domain.Enums;

namespace Masar.Infrastructure.Config
{
    public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
    {
        public void Configure(EntityTypeBuilder<JobApplication> builder)
        {
            builder.HasKey(ja => ja.Id);

            // Prevent a candidate from applying to the same job twice
            builder.HasIndex(ja => new { ja.JobId, ja.CandidateProfileId })
                .IsUnique();

            builder.Property(ja => ja.Status)
                .IsRequired();

            builder.Property(ja => ja.AppliedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(ja => ja.Job)
                .WithMany(j => j.JobApplications)
                .HasForeignKey(ja => ja.JobId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(ja => ja.Candidate)
                .WithMany(c => c.JobApplications)
                .HasForeignKey(ja => ja.CandidateProfileId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
