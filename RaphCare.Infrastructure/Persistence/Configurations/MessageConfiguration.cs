using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Communication;

namespace RaphCare.Infrastructure.Persistence.Configurations;

/// <summary>EF Core Fluent API configuration for the Message entity (communication / clinical context).</summary>
public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RecipientUserId).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Channel).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Subject).HasMaxLength(500);
        builder.Property(e => e.Body).IsRequired().HasMaxLength(8000);
        builder.HasIndex(e => e.ClinicId);
    }
}
