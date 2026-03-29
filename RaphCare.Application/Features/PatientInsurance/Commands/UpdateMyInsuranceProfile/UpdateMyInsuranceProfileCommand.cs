using MediatR;

namespace RaphCare.Application.Features.PatientInsurance.Commands.UpdateMyInsuranceProfile;

public class UpdateMyInsuranceProfileCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
}
