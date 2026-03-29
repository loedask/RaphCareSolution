namespace RaphCare.Mobile.Core.Features.CareTelehealth.Models;

public sealed class TeleSessionListDisplayItem
{
    public Guid Id { get; init; }
    public string PrimaryLine { get; init; } = string.Empty;
    public string SecondaryLine { get; init; } = string.Empty;
}
