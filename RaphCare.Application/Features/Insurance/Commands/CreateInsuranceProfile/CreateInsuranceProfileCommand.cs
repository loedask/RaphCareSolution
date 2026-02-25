using MediatR;

namespace RaphCare.Application.Features.Insurance.Commands.CreateInsuranceProfile;

public class CreateInsuranceProfileCommand : IRequest<Guid>
{
    public Guid PatientId { get; set; }
    public Guid InsurancePlanId { get; set; }
    public string MembershipNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
}

