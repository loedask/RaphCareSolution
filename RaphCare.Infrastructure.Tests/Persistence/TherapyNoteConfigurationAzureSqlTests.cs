using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using RaphCare.Domain.MentalHealth;
using RaphCare.Infrastructure.Persistence.Configurations;
using Xunit;

namespace RaphCare.Infrastructure.Tests.Persistence;

/// <summary>
/// Azure SQL rejects nvarchar(n) when n &gt; 4000 (error 2717). Therapy notes must stay within that limit or use nvarchar(max).
/// </summary>
public sealed class TherapyNoteConfigurationAzureSqlTests
{
    [Fact]
    public void TherapyNoteNotesMaxLengthMustFitAzureSqlNvarcharLimit()
    {
        var conventionSet = SqlServerConventionSetBuilder.Build();
        var modelBuilder = new ModelBuilder(conventionSet);
        new TherapyNoteConfiguration().Configure(modelBuilder.Entity<TherapyNote>());
        var model = modelBuilder.FinalizeModel();

        var maxLength = model.FindEntityType(typeof(TherapyNote))!
            .FindProperty(nameof(TherapyNote.Notes))!
            .GetMaxLength();

        Assert.NotNull(maxLength);
        Assert.True(
            maxLength.Value <= 4000,
            $"TherapyNote.Notes HasMaxLength({maxLength}) exceeds Azure SQL nvarchar(n) limit of 4000.");
    }
}
