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

            // Unique constraint on UserId to ensure one-to-one relationship
            builder.HasIndex(cp => cp.UserId)
                .IsUnique();

            builder.Property(cp => cp.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(cp => cp.LogoUrl)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(cp => cp.Description)
                .IsRequired(true)
                .HasMaxLength(2000);

            builder.Property(cp => cp.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            // One-to-One: ApplicationUser -> CompanyProfile
            builder.HasOne(cp => cp.User)
                .WithOne()
                .HasForeignKey<CompanyProfile>(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many: CompanyProfile -> Jobs
            builder.HasMany(cp => cp.Jobs)
                .WithOne(j => j.Company)
                .HasForeignKey(j => j.CompanyProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-One: CompanyProfile -> ContactInfo
            builder.HasOne(cp => cp.ContactInfo)
                .WithOne(ci => ci.CompanyProfile)
                .HasForeignKey<CompanyContactInfo>(ci => ci.CompanyProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many: CompanyProfile -> ProfessionalLinks
            builder.HasMany(cp => cp.ProfessionalLinks)
                .WithOne(pl => pl.CompanyProfile)
                .HasForeignKey(pl => pl.CompanyProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
