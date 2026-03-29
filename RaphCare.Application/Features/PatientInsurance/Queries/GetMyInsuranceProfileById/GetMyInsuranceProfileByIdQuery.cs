using MediatR;
using RaphCare.Application.Features.PatientInsurance.DTOs;

namespace RaphCare.Application.Features.PatientInsurance.Queries.GetMyInsuranceProfileById;

public class GetMyInsuranceProfileByIdQuery : IRequest<PatientInsuranceProfileDto>
{
    public Guid Id { get; set; }
}
