using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitPrescription;

public sealed class CreateAdminClinicVisitPrescriptionCommand : IRequest<AdminClinicVisitPrescriptionDto?>
{
    public Guid ClinicId { get; set; }
    public Guid VisitId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public int DurationDays { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyList<CreateAdminClinicVisitPrescriptionLine> Items { get; set; } =
        Array.Empty<CreateAdminClinicVisitPrescriptionLine>();
}

public sealed class CreateAdminClinicVisitPrescriptionLine
{
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public int DurationDays { get; set; }
}
