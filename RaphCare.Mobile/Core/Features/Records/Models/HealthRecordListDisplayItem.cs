namespace RaphCare.Mobile.Core.Features.Records.Models;

public sealed class HealthRecordListDisplayItem
{
    public Guid Id { get; init; }
    public string PrimaryLine { get; init; } = string.Empty;
    public string SecondaryLine { get; init; } = string.Empty;
}
