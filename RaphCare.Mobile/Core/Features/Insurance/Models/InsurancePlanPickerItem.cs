namespace RaphCare.Mobile.Core.Features.Insurance.Models;

public sealed class InsurancePlanPickerItem
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;

    public override string ToString() => DisplayName;
}
