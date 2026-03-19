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

            //------------------------------------------
            builder.Property(j => j.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(j => j.JobType)
                .IsRequired();

            builder.Property(j => j.Department)
                .IsRequired();

            builder.Property(j => j.Location)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(j => j.WorkMode)
                .IsRequired();

            //------------------------------------------
            builder.Property(j => j.MinSalary)
                .HasColumnType("decimal(18,2)");

            builder.Property(j => j.MaxSalary)
                .HasColumnType("decimal(18,2)");

            //------------------------------------------
            builder.Property(j => j.Description)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(j => j.Requirements)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(j => j.Benefits)
                .IsRequired(false)
                .HasMaxLength(4000);

            //------------------------------------------
            builder.Property(j => j.ApplicationDeadline)
                .IsRequired();

            builder.Property(j => j.NumberOfOpenings)
                .IsRequired();

            //------------------------------------------
            builder.Property(j => j.IsActive)
                .HasDefaultValue(true);

            builder.Property(j => j.IsFeatured)
                .HasDefaultValue(false);

            builder.Property(j => j.PostedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // One to Many Relation
            builder.HasMany(j => j.JobApplications)
                .WithOne(ja => ja.Job)
                .HasForeignKey(ja => ja.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            // One to Many Relation
            builder.HasMany(j => j.SavedJobs)
                .WithOne(sj => sj.Job)
                .HasForeignKey(sj => sj.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            //------------------------------------------
            builder.HasIndex(j => j.CompanyProfileId);
            builder.HasIndex(j => j.Location);
            builder.HasIndex(j => j.JobType);
            builder.HasIndex(j => j.Department);
            builder.HasIndex(j => j.PostedDate);
        }
    }
}
