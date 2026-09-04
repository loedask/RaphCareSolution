using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.MentalHealth;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class AssessmentResponseConfiguration : IEntityTypeConfiguration<AssessmentResponse>
{
    public void Configure(EntityTypeBuilder<AssessmentResponse> builder)
    {
        builder.ToTable("AssessmentResponses");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ResponseValue).IsRequired().HasMaxLength(200);
        builder.HasIndex(e => e.MentalHealthAssessmentId);
        builder.HasIndex(e => e.AssessmentQuestionId);
        builder.HasIndex(e => new { e.MentalHealthAssessmentId, e.AssessmentQuestionId }).IsUnique();
    }
}
