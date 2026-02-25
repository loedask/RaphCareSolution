using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Service offered by a clinic or by a specific provider.
/// </summary>
public class ServiceOffering : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Guid? ProviderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsTelemedicineAvailable { get; set; }
    public bool IsActive { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public Provider? Provider { get; set; }
}
