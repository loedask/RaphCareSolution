namespace RaphCare.Mobile.Core.Features.Insurance.Models;

public sealed class InsuranceProfileListDisplayItem
{
    public Guid Id { get; init; }
    public string PrimaryLine { get; init; } = string.Empty;
    public string SecondaryLine { get; init; } = string.Empty;
}
