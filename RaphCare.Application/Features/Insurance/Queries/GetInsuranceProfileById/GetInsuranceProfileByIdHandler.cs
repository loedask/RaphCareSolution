using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Insurance.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Insurance.Queries.GetInsuranceProfileById;

public class GetInsuranceProfileByIdHandler : IRequestHandler<GetInsuranceProfileByIdQuery, InsuranceProfileDto>
{
    private readonly IRepository<InsuranceProfile> _repository;

    public GetInsuranceProfileByIdHandler(IRepository<InsuranceProfile> repository)
    {
        _repository = repository;
    }

    public async Task<InsuranceProfileDto> Handle(GetInsuranceProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (profile is null)
        {
            throw new NotFoundException(nameof(InsuranceProfile), request.Id);
        }

        return new InsuranceProfileDto
        {
            Id = profile.Id,
            PatientId = profile.PatientId,
            InsurancePlanId = profile.InsurancePlanId,
            MembershipNumber = profile.MembershipNumber,
            StartDate = profile.StartDate,
            EndDate = profile.EndDate,
            IsActive = profile.IsActive
        };
    }
}

