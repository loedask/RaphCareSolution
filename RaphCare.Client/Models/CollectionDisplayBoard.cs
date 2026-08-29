namespace RaphCare.Client.Models;

public sealed class CollectionDisplayBoard
{
    public string ClinicName { get; set; } = string.Empty;
    public CollectionDisplayTicket? NowServing { get; set; }
    public IReadOnlyList<CollectionDisplayTicket> Waiting { get; set; } =
        Array.Empty<CollectionDisplayTicket>();
}

public sealed class CollectionDisplayTicket
{
    public string PickupCode { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;

    public bool IsLab => string.Equals(Kind, "lab", StringComparison.OrdinalIgnoreCase);
}
