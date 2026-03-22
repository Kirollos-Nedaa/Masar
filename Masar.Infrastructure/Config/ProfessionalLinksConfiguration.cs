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
    public class ProfessionalLinksConfiguration : IEntityTypeConfiguration<ProfessionalLink>
    {
        public void Configure(EntityTypeBuilder<ProfessionalLink> builder)
        {
            builder.HasKey(pl => pl.Id);

            builder.Property(pl => pl.Url)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasOne(pl => pl.CandidateProfile)
                .WithMany(cp => cp.ProfessionalLinks)
                .HasForeignKey(pl => pl.CandidateProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pl => pl.CompanyProfile)
                .WithMany(cp => cp.ProfessionalLinks)
                .HasForeignKey(pl => pl.CompanyProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
