using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Identity;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class LoginAuditConfiguration : IEntityTypeConfiguration<LoginAudit>
{
    public void Configure(EntityTypeBuilder<LoginAudit> builder)
    {
        builder.ToTable("LoginAudit");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.LoginTime).IsRequired();
        builder.Property(e => e.Success).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.IpAddress).HasMaxLength(256);
        builder.Property(e => e.UserAgent).HasMaxLength(256);

        builder.HasIndex(e => e.UserId);

        builder.HasOne(e => e.User)
            .WithMany(u => u.LoginAudits)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
