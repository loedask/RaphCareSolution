namespace RaphCare.Application.Common.Configuration;

/// <summary>Paystack REST API for card (and supported mobile money) charges. Bound from <see cref="SectionName"/>.</summary>
public sealed class PaystackOptions
{
    public const string SectionName = "Paystack";

    /// <summary>Secret key (sk_test_… or sk_live_…).</summary>
    public string? SecretKey { get; set; }

    /// <summary>Public key (pk_test_… or pk_live_…) for clients that need it.</summary>
    public string? PublicKey { get; set; }

    /// <summary>Default callback URL after the Paystack checkout page (optional).</summary>
    public string? CallbackUrl { get; set; }

    public bool IsEnabled => !string.IsNullOrWhiteSpace(SecretKey);
}
