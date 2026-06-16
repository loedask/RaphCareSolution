using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Identity;

namespace RaphCare.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for local email/password credentials.</summary>
public class EmailPasswordCredentialConfiguration : IEntityTypeConfiguration<EmailPasswordCredential>
{
    public void Configure(EntityTypeBuilder<EmailPasswordCredential> builder)
    {
        builder.ToTable("EmailPasswordCredentials");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.PasswordHash).IsRequired().HasMaxLength(1024);
        builder.HasIndex(e => e.Email).IsUnique();

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
