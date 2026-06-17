namespace RaphCare.Application.Common.Configuration;

/// <summary>Firebase Cloud Messaging (service account) for <see cref="IPatientPushNotificationSender"/>.</summary>
public sealed class FirebasePushOptions
{
    public const string SectionName = "FirebasePush";

    /// <summary>Path to the Firebase service account JSON file (readable by the API process).</summary>
    public string? ServiceAccountJsonPath { get; set; }

    /// <summary>True when a credential file path is configured and the file exists.</summary>
    public bool IsEnabled =>
        !string.IsNullOrWhiteSpace(ServiceAccountJsonPath)
        && File.Exists(ServiceAccountJsonPath!);
}
