namespace RaphCare.Client.Models;

public sealed class ConsultDisplayBoard
{
    public string ClinicName { get; set; } = string.Empty;
    public ConsultDisplayTicket? NowServing { get; set; }
    public IReadOnlyList<ConsultDisplayTicket> Waiting { get; set; } =
        Array.Empty<ConsultDisplayTicket>();
}

public sealed class ConsultDisplayTicket
{
    public string QueueCode { get; set; } = string.Empty;
}
