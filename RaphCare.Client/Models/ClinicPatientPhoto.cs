namespace RaphCare.Client.Models;

/// <summary>Private profile photo bytes for a hospital patient chart.</summary>
public sealed class ClinicPatientPhoto
{
    public required byte[] Bytes { get; init; }
    public required string ContentType { get; init; }
}
