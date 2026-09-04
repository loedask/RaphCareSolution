namespace RaphCare.Application.Common.Interfaces;

/// <summary>Stores patient profile photos in private object storage.</summary>
public interface IPatientProfilePhotoStorage
{
    /// <summary>Persists or replaces the photo for <paramref name="patientId"/>.</summary>
    Task<PatientProfilePhotoSaveResult> SaveAsync(
        Guid patientId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>Opens a read stream when a photo exists; otherwise <see langword="null"/>.</summary>
    Task<PatientProfilePhotoReadResult?> OpenReadAsync(Guid patientId, CancellationToken cancellationToken = default);

    /// <summary>Deletes stored photo files for <paramref name="patientId"/> if present.</summary>
    Task DeleteAsync(Guid patientId, CancellationToken cancellationToken = default);
}

/// <summary>Result of saving a profile photo.</summary>
public sealed class PatientProfilePhotoSaveResult
{
    public required string RelativePath { get; init; }
    public required string ContentType { get; init; }
}

/// <summary>Readable profile photo payload.</summary>
public sealed class PatientProfilePhotoReadResult
{
    public required Stream Content { get; init; }
    public required string ContentType { get; init; }
}
