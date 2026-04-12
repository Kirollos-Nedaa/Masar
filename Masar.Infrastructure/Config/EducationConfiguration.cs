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
    public class EducationConfiguration : IEntityTypeConfiguration<Education>
    {
        public void Configure(EntityTypeBuilder<Education> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.University)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Degree)
                .HasMaxLength(200);

            builder.Property(e => e.Major)
                .HasMaxLength(200);

            builder.Property(e => e.StartYear)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(e => e.ExpectedGraduation)
                .IsRequired()
                .HasColumnType("date");
        }
    }
}
