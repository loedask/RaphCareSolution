using MediatR;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth.Commands.CreateBehavioralCarePlan;

public sealed class CreateBehavioralCarePlanCommand : IRequest<BehavioralCarePlanDto?>
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IReadOnlyList<CreateBehavioralCarePlanGoal> Goals { get; set; } =
        Array.Empty<CreateBehavioralCarePlanGoal>();
}

public sealed class CreateBehavioralCarePlanGoal
{
    public string GoalDescription { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
}
