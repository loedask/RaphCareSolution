namespace RaphCare.API.Contracts;

/// <summary>
/// Response body for POST /api/Patients (201 Created). Used for OpenAPI/Swagger only.
/// </summary>
public class CreatePatientResponse
{
    public Guid Id { get; set; }
}
