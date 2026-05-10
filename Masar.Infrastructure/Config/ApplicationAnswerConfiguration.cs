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
    public class ApplicationAnswerConfiguration : IEntityTypeConfiguration<ApplicationAnswer>
    {
        public void Configure(EntityTypeBuilder<ApplicationAnswer> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.AnswerText)
                .IsRequired()
                .HasMaxLength(4000);

            builder.HasOne(a => a.JobQuestion)
                .WithMany()
                .HasForeignKey(a => a.JobQuestionId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
