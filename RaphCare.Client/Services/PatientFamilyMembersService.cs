using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Family;

namespace RaphCare.Client.Services;

public sealed class PatientFamilyMembersService(IHttpClientFactory httpClientFactory) : IPatientFamilyMembersService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private HttpClient Client => httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);

    public async Task<Response<IReadOnlyList<PatientFamilyMemberViewModel>>> GetMyFamilyMembersAsync(CancellationToken cancellationToken = default)
    {
        using var r = await Client.GetAsync("api/patient/family-members", cancellationToken).ConfigureAwait(false);
        if (!r.IsSuccessStatusCode)
            return await Failure<IReadOnlyList<PatientFamilyMemberViewModel>>(r, cancellationToken).ConfigureAwait(false);
        var list = await r.Content.ReadFromJsonAsync<List<MemberJson>>(JsonOptions, cancellationToken).ConfigureAwait(false);
        var vm = (list ?? []).Select(Map).ToList();
        return Response<IReadOnlyList<PatientFamilyMemberViewModel>>.Success(vm);
    }

    public async Task<Response<PatientFamilyMemberViewModel>> GetMyFamilyMemberAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var r = await Client.GetAsync($"api/patient/family-members/{id:N}", cancellationToken).ConfigureAwait(false);
        if (!r.IsSuccessStatusCode)
            return await Failure<PatientFamilyMemberViewModel>(r, cancellationToken).ConfigureAwait(false);
        var x = await r.Content.ReadFromJsonAsync<MemberJson>(JsonOptions, cancellationToken).ConfigureAwait(false);
        return x is null
            ? Response<PatientFamilyMemberViewModel>.Failure("Empty response.", (int)r.StatusCode)
            : Response<PatientFamilyMemberViewModel>.Success(Map(x));
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
        using var r = await Client.PostAsJsonAsync(
            "api/patient/family-members",
            new
            {
                firstName,
                lastName,
                relationship,
                dateOfBirth,
                phoneNumber,
                email,
                linkedPatientId = (Guid?)null
            },
            JsonOptions,
            cancellationToken).ConfigureAwait(false);
        if (r.StatusCode != HttpStatusCode.Created)
            return await Failure<Guid>(r, cancellationToken).ConfigureAwait(false);
        var body = await r.Content.ReadFromJsonAsync<CreatedIdJson>(JsonOptions, cancellationToken).ConfigureAwait(false);
        return body is null
            ? Response<Guid>.Failure("Empty response.", (int)r.StatusCode)
            : Response<Guid>.Success(body.Id);
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
        using var r = await Client.PutAsJsonAsync(
            $"api/patient/family-members/{id:N}",
            new
            {
                firstName,
                lastName,
                relationship,
                dateOfBirth,
                phoneNumber,
                email,
                linkedPatientId
            },
            JsonOptions,
            cancellationToken).ConfigureAwait(false);
        if (r.StatusCode != HttpStatusCode.NoContent)
            return await Failure<bool>(r, cancellationToken).ConfigureAwait(false);
        return Response<bool>.Success(true);
    }

    public async Task<Response<bool>> RemoveFamilyMemberAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var r = await Client.DeleteAsync($"api/patient/family-members/{id:N}", cancellationToken).ConfigureAwait(false);
        if (r.StatusCode != HttpStatusCode.NoContent)
            return await Failure<bool>(r, cancellationToken).ConfigureAwait(false);
        return Response<bool>.Success(true);
    }

    private static PatientFamilyMemberViewModel Map(MemberJson x) => new()
    {
        Id = x.Id,
        FirstName = x.FirstName ?? string.Empty,
        LastName = x.LastName ?? string.Empty,
        Relationship = x.Relationship ?? string.Empty,
        DateOfBirth = x.DateOfBirth,
        PhoneNumber = x.PhoneNumber,
        Email = x.Email,
        LinkedPatientId = x.LinkedPatientId
    };

    private static async Task<Response<T>> Failure<T>(HttpResponseMessage r, CancellationToken cancellationToken)
    {
        var text = r.Content is null ? string.Empty : await r.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return Response<T>.Failure(string.IsNullOrWhiteSpace(text) ? r.ReasonPhrase ?? "Request failed." : text, (int)r.StatusCode);
    }

    private sealed class MemberJson
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Relationship { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public Guid? LinkedPatientId { get; set; }
    }

    private sealed class CreatedIdJson
    {
        public Guid Id { get; set; }
    }
}
