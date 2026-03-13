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
    public class CompanyProfileConfiguration : IEntityTypeConfiguration<CompanyProfile>
    {
        public void Configure(EntityTypeBuilder<CompanyProfile> builder)
        {
            builder.HasKey(cp => cp.Id);

            builder.Property(cp => cp.CompanyName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(cp => cp.CompanyLogo)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(cp => cp.Description)
                .IsRequired(true)
                .HasMaxLength(2000);

            // One-to-One: ApplicationUser -> CompanyProfile
            builder.HasOne(cp => cp.User)
                .WithOne()
                .HasForeignKey<CompanyProfile>(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many: CompanyProfile -> Jobs
            builder.HasMany<Job>()
                .WithOne(j => j.Company)
                .HasForeignKey(j => j.CompanyProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
