using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Patient address (residential, postal, etc.).
/// </summary>
public class Address : PatientOwnedEntity
{
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? Province { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public bool IsPrimary { get; set; }
}
