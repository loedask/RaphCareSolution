using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Family;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

/// <summary>Wraps generated <see cref="IClient"/> family-member operations and maps to feature view models.</summary>
public sealed class PatientFamilyMembersService(IClient client) : IPatientFamilyMembersService
{
    public async Task<Response<IReadOnlyList<PatientFamilyMemberViewModel>>> GetMyFamilyMembersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var dtos = await client.GetMyPatientFamilyMembersAsync(cancellationToken).ConfigureAwait(false);
            var list = (dtos ?? Array.Empty<PatientFamilyMemberDto>())
                .Select(Map)
                .ToList();
            return Response<IReadOnlyList<PatientFamilyMemberViewModel>>.Success(list);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<IReadOnlyList<PatientFamilyMemberViewModel>>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<PatientFamilyMemberViewModel>> GetMyFamilyMemberAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var d = await client.GetMyPatientFamilyMemberByIdAsync(id, cancellationToken).ConfigureAwait(false);
            return Response<PatientFamilyMemberViewModel>.Success(Map(d));
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<PatientFamilyMemberViewModel>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<Guid>> AddFamilyMemberAsync(
        string firstName,
        string lastName,
        string relationship,
        DateTime? dateOfBirth,
        string? phoneNumber,
        string? email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cmd = new AddMyPatientFamilyMemberCommand
            {
                FirstName = firstName,
                LastName = lastName,
                Relationship = relationship,
                DateOfBirth = dateOfBirth,
                PhoneNumber = phoneNumber,
                Email = email,
                LinkedPatientId = null
            };
            var res = await client.AddMyPatientFamilyMemberAsync(cmd, cancellationToken).ConfigureAwait(false);
            return Response<Guid>.Success(res.Id);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<Guid>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<bool>> UpdateFamilyMemberAsync(
        Guid id,
        string firstName,
        string lastName,
        string relationship,
        DateTime? dateOfBirth,
        string? phoneNumber,
        string? email,
        Guid? linkedPatientId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cmd = new UpdateMyPatientFamilyMemberCommand
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                Relationship = relationship,
                DateOfBirth = dateOfBirth,
                PhoneNumber = phoneNumber,
                Email = email,
                LinkedPatientId = linkedPatientId
            };
            await client.UpdateMyPatientFamilyMemberAsync(id, cmd, cancellationToken).ConfigureAwait(false);
            return Response<bool>.Success(true);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<bool>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<bool>> RemoveFamilyMemberAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await client.RemoveMyPatientFamilyMemberAsync(id, cancellationToken).ConfigureAwait(false);
            return Response<bool>.Success(true);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<bool>.Failure(ex.Message, ex.StatusCode);
        }
    }

    private static PatientFamilyMemberViewModel Map(PatientFamilyMemberDto d) => new()
    {
        Id = d.Id,
        FirstName = d.FirstName ?? string.Empty,
        LastName = d.LastName ?? string.Empty,
        Relationship = d.Relationship ?? string.Empty,
        DateOfBirth = d.DateOfBirth,
        PhoneNumber = d.PhoneNumber,
        Email = d.Email,
        LinkedPatientId = d.LinkedPatientId
    };
}
