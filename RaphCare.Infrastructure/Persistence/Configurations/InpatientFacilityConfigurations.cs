using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Organization;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class WardConfiguration : IEntityTypeConfiguration<Ward>
{
    public void Configure(EntityTypeBuilder<Ward> builder)
    {
        builder.ToTable("Wards");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Code).HasMaxLength(50);
        builder.Property(e => e.IsActive).IsRequired();
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => e.FacilityId);
        builder.HasOne(e => e.Facility)
            .WithMany()
            .HasForeignKey(e => e.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Clinic)
            .WithMany()
            .HasForeignKey(e => e.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.RoomType).HasMaxLength(100);
        builder.Property(e => e.IsActive).IsRequired();
        builder.HasIndex(e => e.WardId);
        builder.HasOne(e => e.Ward)
            .WithMany(w => w.Rooms)
            .HasForeignKey(e => e.WardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class BedConfiguration : IEntityTypeConfiguration<Bed>
{
    public void Configure(EntityTypeBuilder<Bed> builder)
    {
        builder.ToTable("Beds");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Label).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(50);
        builder.Property(e => e.IsActive).IsRequired();
        builder.HasIndex(e => e.RoomId);
        builder.HasIndex(e => e.Status);
        builder.HasOne(e => e.Room)
            .WithMany(r => r.Beds)
            .HasForeignKey(e => e.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
