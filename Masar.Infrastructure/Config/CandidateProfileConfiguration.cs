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
    public class CandidateProfileConfiguration : IEntityTypeConfiguration<CandidateProfile>
    {
        public void Configure(EntityTypeBuilder<CandidateProfile> builder)
        {
            builder.HasKey(cp => cp.Id);

            builder.Property(cp => cp.UserId)
                .IsRequired();

            builder.HasIndex(cp => cp.UserId)
                .IsUnique();

            builder.Property(cp => cp.Bio)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(cp => cp.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(cp => cp.Location)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(cp => cp.DateOfBirth)
                .IsRequired(false)
                .HasColumnType("date");

            builder.Property(cp => cp.Gender)
                .IsRequired(false);

            builder.Property(cp => cp.ResumeUrl)
                .IsRequired(false);

            builder.HasOne(cp => cp.User)
                .WithOne(u => u.CandidateProfile)
                .HasForeignKey<CandidateProfile>(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-many relationships
            builder.HasMany(cp => cp.Educations)
                .WithOne(e => e.CandidateProfile)
                .HasForeignKey(e => e.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-many relationships
            builder.HasMany(cp => cp.CandidateSkills)
                .WithOne(cs => cs.CandidateProfile)
                .HasForeignKey(cs => cs.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-many relationships
            builder.HasMany(cp => cp.ProfessionalLinks)
                .WithOne(pl => pl.CandidateProfile)
                .HasForeignKey(pl => pl.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-many relationships
            builder.HasMany(cp => cp.JobApplications)
                .WithOne(ja => ja.Candidate)
                .HasForeignKey(ja => ja.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-many relationships
            builder.HasMany(cp => cp.SavedJobs)
                .WithOne(sj => sj.Candidate)
                .HasForeignKey(sj => sj.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            //------------------------------------------
            builder.HasIndex(cp => cp.UserId);
            builder.HasIndex(cp => cp.Location);
        }
    }
}
