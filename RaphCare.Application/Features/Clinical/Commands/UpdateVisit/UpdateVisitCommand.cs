using MediatR;

namespace RaphCare.Application.Features.Clinical.Commands.UpdateVisit;

public class UpdateVisitCommand : IRequest
{
    public Guid Id { get; set; }
    public DateTime? VisitEnd { get; set; }
    public string? Status { get; set; }
    public string? Summary { get; set; }
}

