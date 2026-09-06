using FluentValidation.TestHelper;
using RaphCare.Application.Features.PatientAiAssistant.Commands.SendMyPatientAssistantMessage;
using RaphCare.Application.Features.PatientAiAssistant.DTOs;
using Xunit;

namespace RaphCare.Application.Tests.Features.PatientAiAssistant.Commands.SendMyPatientAssistantMessage;

public sealed class SendMyPatientAssistantMessageValidatorTests
{
    private readonly SendMyPatientAssistantMessageValidator _validator = new();

    [Fact]
    public void ValidMessageWithPriorPasses()
    {
        var command = new SendMyPatientAssistantMessageCommand
        {
            Message = "How can I sleep better?",
            PriorMessages =
            [
                new PatientAssistantPriorMessageDto { Role = "assistant", Content = "Hello" },
                new PatientAssistantPriorMessageDto { Role = "user", Content = "I feel tired" },
            ],
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void InvalidPriorRoleFails()
    {
        var command = new SendMyPatientAssistantMessageCommand
        {
            Message = "Hello",
            PriorMessages =
            [
                new PatientAssistantPriorMessageDto { Role = "system", Content = "nope" },
            ],
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("PriorMessages[0].Role");
    }

    [Fact]
    public void TooManyPriorMessagesFails()
    {
        var command = new SendMyPatientAssistantMessageCommand
        {
            Message = "Hello",
            PriorMessages = Enumerable.Range(1, 13)
                .Select(i => new PatientAssistantPriorMessageDto { Role = "user", Content = $"m{i}" })
                .ToList(),
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PriorMessages);
    }
}
