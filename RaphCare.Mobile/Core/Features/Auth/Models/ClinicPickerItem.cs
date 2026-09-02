namespace RaphCare.Mobile.Core.Features.Auth.Models;

public sealed class ClinicPickerItem
{
    public Guid? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ReferenceCode { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;

    public override string ToString() => DisplayName;
}
