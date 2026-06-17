using RaphCare.Application.Common.DTOs;

namespace RaphCare.Application.Features.AI.DTOs;

public class AISummaryDto : BaseDto
{
    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public string SummaryText { get; set; } = string.Empty;
}

