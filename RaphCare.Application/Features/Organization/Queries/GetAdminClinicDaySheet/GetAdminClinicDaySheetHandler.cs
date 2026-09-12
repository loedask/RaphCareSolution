using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Billing;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicDaySheet;

public sealed class GetAdminClinicDaySheetHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Invoice> invoiceRepository,
    IRepository<InvoiceLineItem> invoiceLineItemRepository,
    IRepository<Patient> patientRepository,
    IDateTimeProvider clock)
    : IRequestHandler<GetAdminClinicDaySheetQuery, IReadOnlyList<AdminClinicDaySheetItemDto>?>
{
    public async Task<IReadOnlyList<AdminClinicDaySheetItemDto>?> Handle(
        GetAdminClinicDaySheetQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can view the day sheet.");

        var dayStart = clock.UtcNow.Date;
        var dayEnd = dayStart.AddDays(1);

        var invoicesPage = await invoiceRepository.SearchAsync(
            q => q.Where(i =>
                i.ClinicId == request.ClinicId
                && i.VisitId != null
                && (
                    (i.DueDate >= dayStart && i.DueDate < dayEnd)
                    || (i.PaidAt != null && i.PaidAt >= dayStart && i.PaidAt < dayEnd)
                    || (i.CreatedAt >= dayStart && i.CreatedAt < dayEnd))),
            1,
            500,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var invoices = invoicesPage.Items
            .OrderByDescending(i => i.PaidAt ?? i.CreatedAt)
            .ToList();
        if (invoices.Count == 0)
            return Array.Empty<AdminClinicDaySheetItemDto>();

        var invoiceIds = invoices.Select(i => i.Id).ToList();
        var linesPage = await invoiceLineItemRepository.SearchAsync(
            q => q.Where(l => invoiceIds.Contains(l.InvoiceId)),
            1,
            2000,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);
        var descriptionByInvoice = linesPage.Items
            .GroupBy(l => l.InvoiceId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(l => l.CreatedAt).Select(l => l.Description).FirstOrDefault() ?? "Consultation");

        var patientIds = invoices.Select(i => i.PatientId).Distinct().ToList();
        var patientsPage = await patientRepository.SearchAsync(
            q => q.Where(p => patientIds.Contains(p.Id)),
            1,
            Math.Max(patientIds.Count, 1),
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);
        var patientsById = patientsPage.Items.ToDictionary(p => p.Id);

        return invoices.Select(invoice =>
        {
            patientsById.TryGetValue(invoice.PatientId, out var patient);
            var patientName = patient is null
                ? "Patient"
                : $"{patient.FirstName} {patient.LastName}".Trim();
            descriptionByInvoice.TryGetValue(invoice.Id, out var description);
            return new AdminClinicDaySheetItemDto
            {
                InvoiceId = invoice.Id,
                VisitId = invoice.VisitId!.Value,
                PatientId = invoice.PatientId,
                PatientName = patientName,
                Amount = invoice.Amount,
                Currency = invoice.Currency,
                Status = invoice.Status,
                PaidAt = invoice.PaidAt,
                Description = description ?? "Consultation"
            };
        }).ToList();
    }
}
