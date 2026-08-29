namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class CollectionDisplayBoardDto
{
    public const string PrescriptionKind = "prescription";
    public const string LabKind = "lab";

    public string ClinicName { get; set; } = string.Empty;
    public CollectionDisplayTicketDto? NowServing { get; set; }
    public IReadOnlyList<CollectionDisplayTicketDto> Waiting { get; set; } =
        Array.Empty<CollectionDisplayTicketDto>();
}

public sealed class CollectionDisplayTicketDto
{
    public string PickupCode { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
}

public sealed class CollectionDisplayLinkDto
{
    public string Token { get; set; } = string.Empty;
}
