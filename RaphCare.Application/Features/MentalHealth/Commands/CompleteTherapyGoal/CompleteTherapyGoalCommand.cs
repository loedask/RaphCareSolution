using MediatR;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth.Commands.CompleteTherapyGoal;

public sealed class CompleteTherapyGoalCommand : IRequest<TherapyGoalDto?>
{
    public Guid ClinicId { get; set; }
    public Guid CarePlanId { get; set; }
    public Guid GoalId { get; set; }
    public bool IsCompleted { get; set; } = true;
}
