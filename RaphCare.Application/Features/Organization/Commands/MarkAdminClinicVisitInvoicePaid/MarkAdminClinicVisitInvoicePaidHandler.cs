using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Billing;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.MarkAdminClinicVisitInvoicePaid;

public sealed class MarkAdminClinicVisitInvoicePaidHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Invoice> invoiceRepository,
    IRepository<InvoiceLineItem> invoiceLineItemRepository,
    IRepository<Patient> patientRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<MarkAdminClinicVisitInvoicePaidCommand, AdminClinicDaySheetItemDto?>
{
    public async Task<AdminClinicDaySheetItemDto?> Handle(
        MarkAdminClinicVisitInvoicePaidCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can mark visit invoices as paid.");

        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken).ConfigureAwait(false);
        if (invoice is null || invoice.ClinicId != request.ClinicId || invoice.VisitId is null)
            return null;

        if (!string.Equals(invoice.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Only pending visit invoices can be marked as paid.");

        var paidAt = clock.UtcNow;
        invoice.Status = "Paid";
        invoice.PaymentMethod = "Cash";
        invoice.PaidAt = paidAt;

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var patient = await patientRepository.GetByIdAsync(invoice.PatientId, cancellationToken).ConfigureAwait(false);
        var linesPage = await invoiceLineItemRepository.SearchAsync(
            q => q.Where(l => l.InvoiceId == invoice.Id).OrderBy(l => l.CreatedAt),
            1,
            1,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);
        var description = linesPage.Items.Count > 0
            ? linesPage.Items[0].Description
            : "Consultation";

        return new AdminClinicDaySheetItemDto
        {
            InvoiceId = invoice.Id,
            VisitId = invoice.VisitId.Value,
            PatientId = invoice.PatientId,
            PatientName = patient is null ? "Patient" : $"{patient.FirstName} {patient.LastName}".Trim(),
            Amount = invoice.Amount,
            Currency = invoice.Currency,
            Status = invoice.Status,
            PaidAt = invoice.PaidAt,
            Description = description
        };
    }
}
