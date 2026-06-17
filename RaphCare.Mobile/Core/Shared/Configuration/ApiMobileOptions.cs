namespace RaphCare.Mobile.Core.Shared.Configuration;

/// <summary>Mobile API client settings (base URL is also read via <c>Api:BaseAddress</c> in DI).</summary>
public sealed class ApiMobileOptions
{
    public const string SectionName = "Api";

    /// <summary>
    /// Clinic Guid sent as <c>X-Clinic-Id</c> on every API call. Required for multi-tenant endpoints.
    /// Use the demo clinic id from your database (see ClinicalSeeder) or set via User Secrets.
    /// </summary>
    public Guid? ClinicId { get; set; }
}
