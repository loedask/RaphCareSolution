namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class AdminClinicCollectionBoardDto
{
    public IReadOnlyList<AdminClinicCollectionPrescriptionDto> Prescriptions { get; set; } =
        Array.Empty<AdminClinicCollectionPrescriptionDto>();

    public IReadOnlyList<AdminClinicCollectionLabOrderDto> LabOrders { get; set; } =
        Array.Empty<AdminClinicCollectionLabOrderDto>();

    public IReadOnlyList<AdminClinicCollectionPrescriptionDto> RecentPrescriptions { get; set; } =
        Array.Empty<AdminClinicCollectionPrescriptionDto>();

    public IReadOnlyList<AdminClinicCollectionLabOrderDto> RecentLabOrders { get; set; } =
        Array.Empty<AdminClinicCollectionLabOrderDto>();
}

public sealed class AdminClinicCollectionPrescriptionDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? NationalHealthId { get; set; }
    public string PickupCode { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? DispensedAt { get; set; }
    public DateTime? CalledAt { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyList<AdminClinicVisitPrescriptionItemDto> Items { get; set; } =
        Array.Empty<AdminClinicVisitPrescriptionItemDto>();
}

public sealed class AdminClinicCollectionLabOrderDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? NationalHealthId { get; set; }
    public string PickupCode { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime RequestedAt { get; set; }
    public DateTime? CalledAt { get; set; }
}
