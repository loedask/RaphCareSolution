using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.MentalHealth;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class AssessmentQuestionConfiguration : IEntityTypeConfiguration<AssessmentQuestion>
{
    public void Configure(EntityTypeBuilder<AssessmentQuestion> builder)
    {
        builder.ToTable("AssessmentQuestions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.QuestionText).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.Order).IsRequired();
        builder.HasIndex(e => e.MentalHealthAssessmentId);
        builder.HasIndex(e => new { e.MentalHealthAssessmentId, e.Order }).IsUnique();
        builder.HasMany(e => e.Responses)
            .WithOne(r => r.Question)
            .HasForeignKey(r => r.AssessmentQuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
