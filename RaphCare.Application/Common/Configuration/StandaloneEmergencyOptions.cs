namespace RaphCare.Application.Common.Configuration;

/// <summary>4G / OEM webhook ingestion (Y6-class standalone devices). Bound from <see cref="SectionName"/>.</summary>
public sealed class StandaloneEmergencyOptions
{
    public const string SectionName = "StandaloneEmergency";

    /// <summary>Shared secret for HMAC-SHA256 over the raw UTF-8 request body (hex digest in header).</summary>
    public string? WebhookSharedSecret { get; set; }

    /// <summary>When true and the host environment is Development, requests without a valid signature are still accepted.</summary>
    public bool AllowUnsignedWebhooksInDevelopment { get; set; } = true;
}
