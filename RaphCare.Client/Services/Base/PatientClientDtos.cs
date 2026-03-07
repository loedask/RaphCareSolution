using System.Text.Json.Serialization;

namespace RaphCare.Client.Services.Base;

/// <summary>
/// Response DTOs for patient API calls. Used by the partial Client when returning typed results from IClient.
/// </summary>
public class PatientDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("clinicId")]
    public Guid ClinicId { get; set; }

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("dateOfBirth")]
    public DateTime DateOfBirth { get; set; }
}

/// <summary>
/// Paged list of patients from the API.
/// </summary>
public class PagedResultOfPatientDto
{
    [JsonPropertyName("items")]
    public IReadOnlyList<PatientDto> Items { get; set; } = Array.Empty<PatientDto>();

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
}

/// <summary>
/// Create patient response (201 body).
/// </summary>
public class CreatePatientResult
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
}
