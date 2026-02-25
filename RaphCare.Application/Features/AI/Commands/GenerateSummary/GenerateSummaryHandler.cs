using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.AI.DTOs;

namespace RaphCare.Application.Features.AI.Commands.GenerateSummary;

public class GenerateSummaryHandler : IRequestHandler<GenerateSummaryCommand, AISummaryDto>
{
    private readonly IAIService _aiService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserService _currentUserService;

    public GenerateSummaryHandler(
        IAIService aiService,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUserService)
    {
        _aiService = aiService;
        _dateTimeProvider = dateTimeProvider;
        _currentUserService = currentUserService;
    }

    public async Task<AISummaryDto> Handle(GenerateSummaryCommand request, CancellationToken cancellationToken)
    {
        // Input retrieval from domain will be added later.
        var input = $"SourceType={request.SourceType}, SourceId={request.SourceId}";

        var summaryText = await _aiService.GenerateSummaryAsync(input, cancellationToken);

        return new AISummaryDto
        {
            Id = Guid.NewGuid(),
            SourceType = request.SourceType,
            SourceId = request.SourceId,
            SummaryText = summaryText
        };
    }
}

