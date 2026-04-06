using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Profile;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

/// <summary>Wraps generated <see cref="IClient"/> patient profile operations and maps to feature view models.</summary>
public sealed class PatientProfileService(IClient client) : IPatientProfileService
{
    private readonly IClient _client = client;

    public async Task<Response<MyPatientProfileViewModel>> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await _client.GetMyPatientProfileAsync(cancellationToken).ConfigureAwait(false);
            return Response<MyPatientProfileViewModel>.Success(Map(dto));
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<MyPatientProfileViewModel>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<bool>> UpdateMyProfileAsync(MyPatientProfileUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var body = new UpdateMyPatientProfileCommand
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
            };
            await _client.UpdateMyPatientProfileAsync(body, cancellationToken).ConfigureAwait(false);
            return Response<bool>.Success(true);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<bool>.Failure(ex.Message, ex.StatusCode);
        }
    }

    private static MyPatientProfileViewModel Map(MyPatientProfileDto d) =>
        new()
        {
            PatientId = d.PatientId,
            FirstName = d.FirstName ?? string.Empty,
            LastName = d.LastName ?? string.Empty,
            Email = d.Email,
            PhoneNumber = d.PhoneNumber,
            DateOfBirth = d.DateOfBirth,
            Gender = d.Gender ?? string.Empty,
        };
}
