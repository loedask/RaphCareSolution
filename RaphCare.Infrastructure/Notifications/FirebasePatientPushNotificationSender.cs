using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Notifications;

/// <summary>
/// Sends patient push notifications via Firebase Cloud Messaging when <see cref="FirebasePushOptions.ServiceAccountJsonPath"/> is configured.
/// Expects device tokens produced by the mobile app’s FCM registration flow.
/// </summary>
public sealed partial class FirebasePatientPushNotificationSender(
    IPatientPushDeviceTokenReader tokenReader,
    IOptionsMonitor<FirebasePushOptions> options,
    ILogger<FirebasePatientPushNotificationSender> logger) : IPatientPushNotificationSender
{
    private readonly IPatientPushDeviceTokenReader _tokenReader = tokenReader;
    private readonly IOptionsMonitor<FirebasePushOptions> _options = options;
    private readonly ILogger<FirebasePatientPushNotificationSender> _logger = logger;

    private static readonly object FirebaseInitLock = new();
    private static string? _initializedCredentialPath;

    /// <inheritdoc />
    public async Task SendToPatientAsync(
        Guid patientId,
        string title,
        string body,
        string notificationType,
        CancellationToken cancellationToken = default)
    {
        var o = _options.CurrentValue;
        if (!o.IsEnabled)
        {
            LogFirebasePushSkipped("Service account path not configured or missing");
            return;
        }

        var path = o.ServiceAccountJsonPath!.Trim();
        if (!EnsureApp(path))
            return;

        var targets = await _tokenReader.GetTokensForPatientAsync(patientId, cancellationToken).ConfigureAwait(false);
        if (targets.Count == 0)
        {
            LogPushSkippedNoTokens(patientId);
            return;
        }

        var safeTitle = Truncate(title, 200);
        var safeBody = Truncate(body, 1500);

        foreach (var t in targets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var message = new Message
                {
                    Token = t.DeviceToken,
                    Notification = new Notification
                    {
                        Title = safeTitle,
                        Body = safeBody,
                    },
                    Data = new Dictionary<string, string>
                    {
                        ["type"] = Truncate(notificationType, 120),
                    },
                };

                await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken).ConfigureAwait(false);
                LogFcmSent(patientId, t.Platform);
            }
            catch (FirebaseMessagingException ex)
            {
                LogFcmSendFailed(ex, patientId, t.Platform, ex.Message);
            }
        }
    }

    private bool EnsureApp(string credentialPath)
    {
        try
        {
            lock (FirebaseInitLock)
            {
                if (_initializedCredentialPath is not null)
                {
                    if (!string.Equals(_initializedCredentialPath, credentialPath, StringComparison.OrdinalIgnoreCase))
                    {
                        LogFirebaseCredentialPathMismatch();
                    }

                    return true;
                }

                if (FirebaseApp.DefaultInstance is null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = Google.Apis.Auth.OAuth2.CredentialFactory
                            .FromFile<ServiceAccountCredential>(credentialPath)
                            .ToGoogleCredential(),
                    });
                }

                _initializedCredentialPath = credentialPath;
            }

            return true;
        }
        catch (Exception ex)
        {
            LogFirebaseInitFailed(ex, credentialPath);
            return false;
        }
    }

    private static string Truncate(string value, int maxLen)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLen)
            return value;
        return value[..maxLen];
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Firebase push skipped: {Reason}.")]
    private partial void LogFirebasePushSkipped(string reason);

    [LoggerMessage(Level = LogLevel.Information, Message = "Push skipped: no device tokens for patient {PatientId}.")]
    private partial void LogPushSkippedNoTokens(Guid patientId);

    [LoggerMessage(Level = LogLevel.Information, Message = "FCM sent to patient {PatientId} platform {Platform}.")]
    private partial void LogFcmSent(Guid patientId, string platform);

    [LoggerMessage(Level = LogLevel.Warning, Message = "FCM send failed for patient {PatientId} platform {Platform}: {Message}")]
    private partial void LogFcmSendFailed(Exception exception, Guid patientId, string platform, string message);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Firebase was already initialized with a different credential path; continuing with the first app instance.")]
    private partial void LogFirebaseCredentialPathMismatch();

    [LoggerMessage(Level = LogLevel.Error, Message = "FirebaseApp initialization failed for path {Path}.")]
    private partial void LogFirebaseInitFailed(Exception exception, string path);
}
