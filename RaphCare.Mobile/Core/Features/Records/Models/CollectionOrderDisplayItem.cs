using Microsoft.Maui.Controls;

namespace RaphCare.Mobile.Core.Features.Records.Models;

public sealed class CollectionOrderDisplayItem
{
    public Guid VisitId { get; init; }
    public string KindLabel { get; init; } = string.Empty;
    public string ClinicName { get; init; } = string.Empty;
    public string PickupCode { get; init; } = string.Empty;
    public string DetailLine { get; init; } = string.Empty;
    public string StatusText { get; init; } = string.Empty;
    public ImageSource? QrImage { get; init; }
}
