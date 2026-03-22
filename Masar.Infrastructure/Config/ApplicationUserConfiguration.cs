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
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // First Name
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            // Last Name
            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(50);

            // CreatedAt — set once, never updated
            builder.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
