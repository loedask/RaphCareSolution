namespace RaphCare.Application.Features.PatientDevices.Queries.GetMyLatestReadings;

public sealed class GetMyLatestReadingsQuery : MediatR.IRequest<GetMyLatestReadingsResponseDto>;

public sealed class GetMyLatestReadingsResponseDto
{
    public decimal? HeartRateBpm { get; set; }
    public DateTime? HeartRateRecordedAt { get; set; }
    public decimal? SpO2Percent { get; set; }
    public DateTime? SpO2RecordedAt { get; set; }
}
