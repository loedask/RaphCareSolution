using MediatR;
using RaphCare.Application.Features.AI.DTOs;

namespace RaphCare.Application.Features.AI.Commands.GenerateSummary;

public class GenerateSummaryCommand : IRequest<AISummaryDto>
{
    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
}

