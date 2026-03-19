using Masar.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Config
{
    public class JobQuestionConfiguration : IEntityTypeConfiguration<JobQuestion>
    {
        public void Configure(EntityTypeBuilder<JobQuestion> builder)
        {
            builder.HasKey(q => q.Id);

            builder.Property(q => q.QuestionText)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(q => q.Type)
                .IsRequired();

            builder.Property(q => q.IsRequired)
                .IsRequired();

            builder.HasIndex(q => q.JobId);

            builder.HasOne(q => q.Job)
                .WithMany(j => j.JobQuestions)
                .HasForeignKey(q => q.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
