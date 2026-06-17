using MediatR;
using RaphCare.Application.Features.Insurance.DTOs;

namespace RaphCare.Application.Features.Insurance.Queries.GetInsuranceProfileById;

public class GetInsuranceProfileByIdQuery : IRequest<InsuranceProfileDto>
{
    public Guid Id { get; set; }
}

