namespace RaphCare.Application.Common.Interfaces;

/// <summary>Validates <c>X-RaphCare-Emergency-Signature</c> (hex-encoded HMAC-SHA256 of the raw request body).</summary>
public interface IStandaloneEmergencyWebhookSignatureValidator
{
    /// <param name="signatureHeader">Value of the signature header, or null if absent.</param>
    /// <param name="bodyUtf8">Exact UTF-8 bytes of the request body used for HMAC.</param>
    bool IsValid(string? signatureHeader, ReadOnlySpan<byte> bodyUtf8);
}
