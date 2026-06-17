using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Insurance;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

/// <summary>Wraps generated <see cref="IClient"/> patient insurance operations and maps to feature view models.</summary>
public sealed class PatientInsuranceService(IClient client) : IPatientInsuranceService
{
    public async Task<Response<IReadOnlyList<InsurancePlanOptionViewModel>>> GetActivePlansAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var dtos = await client.GetActiveInsurancePlansForPatientAsync(cancellationToken).ConfigureAwait(false);
            var list = (dtos ?? Array.Empty<InsurancePlanOptionDto>())
                .Select(d => new InsurancePlanOptionViewModel
                {
                    Id = d.Id,
                    Name = d.Name ?? string.Empty,
                    Code = d.Code ?? string.Empty
                })
                .ToList();
            return Response<IReadOnlyList<InsurancePlanOptionViewModel>>.Success(list);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<IReadOnlyList<InsurancePlanOptionViewModel>>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<PagedPatientInsuranceProfilesViewModel>> GetMyProfilesAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var paged = await client.GetMyInsuranceProfilesAsync(pageNumber, pageSize, cancellationToken).ConfigureAwait(false);
            return Response<PagedPatientInsuranceProfilesViewModel>.Success(MapPaged(paged));
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<PagedPatientInsuranceProfilesViewModel>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<PatientInsuranceProfileViewModel?>> GetMyProfileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await client.GetMyInsuranceProfileByIdAsync(id, cancellationToken).ConfigureAwait(false);
            return Response<PatientInsuranceProfileViewModel?>.Success(MapProfile(dto));
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<PatientInsuranceProfileViewModel?>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<Guid>> CreateProfileAsync(CreatePatientInsuranceProfileRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var cmd = new CreatePatientInsuranceProfileCommand
            {
                InsurancePlanId = request.InsurancePlanId,
                MembershipNumber = request.MembershipNumber,
                StartDate = request.StartDate
            };
            var res = await client.CreatePatientInsuranceProfileAsync(cmd, cancellationToken).ConfigureAwait(false);
            return Response<Guid>.Success(res.Id);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<Guid>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<bool>> UpdateProfileAsync(UpdatePatientInsuranceProfileRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var cmd = new UpdateMyInsuranceProfileCommand
            {
                Id = request.Id,
                EndDate = request.EndDate,
                IsActive = request.IsActive
            };
            await client.UpdateMyInsuranceProfileAsync(request.Id, cmd, cancellationToken).ConfigureAwait(false);
            return Response<bool>.Success(true);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<bool>.Failure(ex.Message, ex.StatusCode);
        }
    }

    private static PagedPatientInsuranceProfilesViewModel MapPaged(PatientInsuranceProfileDtoPagedResult paged)
    {
        var items = (paged.Items ?? Array.Empty<PatientInsuranceProfileDto>())
            .Select(MapProfile)
            .ToList();

        return new PagedPatientInsuranceProfilesViewModel
        {
            Items = items,
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }

    private static PatientInsuranceProfileViewModel MapProfile(PatientInsuranceProfileDto d) =>
        new()
        {
            Id = d.Id,
            InsurancePlanId = d.InsurancePlanId,
            PlanName = d.PlanName ?? string.Empty,
            PlanCode = d.PlanCode ?? string.Empty,
            MembershipNumber = d.MembershipNumber ?? string.Empty,
            StartDate = d.StartDate,
            EndDate = d.EndDate,
            IsActive = d.IsActive
        };
}
