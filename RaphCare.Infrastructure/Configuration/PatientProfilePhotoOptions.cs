namespace RaphCare.Infrastructure.Configuration;

/// <summary>Host configuration for patient profile photo storage.</summary>
public sealed class PatientProfilePhotoOptions
{
    public const string SectionName = "PatientProfilePhotos";

    /// <summary>Directory under content root (default <c>App_Data/patient-photos</c>).</summary>
    public string StorageSubdirectory { get; set; } = "App_Data/patient-photos";

    public int MaxBytes { get; set; } = 5 * 1024 * 1024;

    public string[] AllowedContentTypes { get; set; } =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];
}
