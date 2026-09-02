namespace RaphCare.Client.Models;

public sealed class CasualtyDisplayBoard
{
    public string ClinicName { get; set; } = string.Empty;
    public CasualtyDisplayTicket? NowServing { get; set; }
    public IReadOnlyList<CasualtyDisplayTicket> Waiting { get; set; } =
        Array.Empty<CasualtyDisplayTicket>();
}

public sealed class CasualtyDisplayTicket
{
    public string QueueCode { get; set; } = string.Empty;
    public string TriageLevel { get; set; } = string.Empty;
}
