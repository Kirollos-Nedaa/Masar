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
    public class ProfessionalLinksConfiguration : IEntityTypeConfiguration<ProfessionalLinks>
    {
        public void Configure(EntityTypeBuilder<ProfessionalLinks> builder)
        {
            builder.HasKey(pl => pl.Id);

            builder.Property(pl => pl.LinkedInUrl)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(pl => pl.GitHubUrl)
                .HasMaxLength(300);

            builder.Property(pl => pl.PortfolioUrl)
                .HasMaxLength(300);
        }
    }
}
