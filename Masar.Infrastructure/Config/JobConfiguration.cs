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
    public class JobConfiguration : IEntityTypeConfiguration<Job>
    {
        public void Configure(EntityTypeBuilder<Job> builder)
        {
            builder.HasKey(j => j.Id);

            builder.Property(j => j.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(j => j.Description)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(j => j.Requirements)
                .HasMaxLength(3000);

            builder.Property(j => j.Location)
                .HasMaxLength(200);

            builder.Property(j => j.JobType)
                .HasMaxLength(50); // "Internship", "Full-time", "Part-time"

            builder.Property(j => j.WorkMode)
                .HasMaxLength(50); // "Remote", "On-site", "Hybrid"

            builder.Property(j => j.MinSalary)
                .HasColumnType("decimal(18,2)");

            builder.Property(j => j.MaxSalary)
                .HasColumnType("decimal(18,2)");

            builder.Property(j => j.Views)
                .HasDefaultValue(0);

            builder.Property(j => j.IsActive)
                .HasDefaultValue(true);

            builder.Property(j => j.IsFeatured)
                .HasDefaultValue(false);

            builder.Property(j => j.PostedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // One-to-Many: Job -> JobApplications
            builder.HasMany<JobApplication>()
                .WithOne(ja => ja.Job)
                .HasForeignKey(ja => ja.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many: Job -> SavedJobs
            builder.HasMany<SavedJob>()
                .WithOne(sj => sj.Job)
                .HasForeignKey(sj => sj.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
