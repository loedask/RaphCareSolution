using RaphCare.Application.Features.MentalHealth.Commands.AddTherapyNote;
using Xunit;

namespace RaphCare.Application.Tests.Features.MentalHealth.Commands.AddTherapyNote;

public sealed class AddTherapyNoteValidatorTests
{
    [Fact]
    public void ValidatorRejectsNotesLongerThanAzureSqlNvarcharLimit()
    {
        var validator = new AddTherapyNoteValidator();
        var result = validator.Validate(new AddTherapyNoteCommand
        {
            ClinicId = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            Notes = new string('x', 4001),
            Category = "Progress"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddTherapyNoteCommand.Notes));
    }

    [Fact]
    public void ValidatorAcceptsNotesAtAzureSqlNvarcharLimit()
    {
        var validator = new AddTherapyNoteValidator();
        var result = validator.Validate(new AddTherapyNoteCommand
        {
            ClinicId = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            Notes = new string('x', 4000),
            Category = "Progress"
        });

        Assert.True(result.IsValid);
    }
}
