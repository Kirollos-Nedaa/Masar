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

            builder.Property(cp => cp.Bio)
                .HasMaxLength(1000);

            builder.Property(cp => cp.Location)
                .HasMaxLength(200);

            builder.Property(cp => cp.ResumeUrl)
                .HasMaxLength(500);

            builder.Property(cp => cp.ProfileViews)
                .HasDefaultValue(0);

            // One-to-One: ApplicationUser -> CandidateProfile
            builder.HasOne(cp => cp.User)
                .WithOne()
                .HasForeignKey<CandidateProfile>(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many: CandidateProfile -> Educations
            builder.HasMany(cp => cp.Educations)
                .WithOne(e => e.CandidateProfile)
                .HasForeignKey(e => e.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many: CandidateProfile -> CandidateSkills
            builder.HasMany(cp => cp.CandidateSkills)
                .WithOne(cs => cs.CandidateProfile)
                .HasForeignKey(cs => cs.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-One: CandidateProfile -> ProfessionalLinks
            builder.HasOne(cp => cp.ProfessionalLinks)
                .WithOne(pl => pl.CandidateProfile)
                .HasForeignKey<ProfessionalLinks>(pl => pl.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
