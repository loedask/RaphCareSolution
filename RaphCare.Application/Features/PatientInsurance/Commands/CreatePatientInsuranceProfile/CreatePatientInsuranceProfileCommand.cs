using MediatR;

namespace RaphCare.Application.Features.PatientInsurance.Commands.CreatePatientInsuranceProfile;

public class CreatePatientInsuranceProfileCommand : IRequest<Guid>
{
    public Guid InsurancePlanId { get; set; }
    public string MembershipNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
}
