using MediatR;

namespace RaphCare.Application.Features.Insurance.Commands.UpdateInsuranceProfile;

public class UpdateInsuranceProfileCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
}

