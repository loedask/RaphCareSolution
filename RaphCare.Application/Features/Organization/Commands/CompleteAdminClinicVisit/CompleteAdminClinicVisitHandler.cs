using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Billing;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicVisit;

public sealed class CompleteAdminClinicVisitHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Visit> visitRepository,
    IRepository<Appointment> appointmentRepository,
    IRepository<VitalSignRecord> vitalRepository,
    IRepository<Patient> patientRepository,
    IRepository<Provider> providerRepository,
    IRepository<Invoice> invoiceRepository,
    IRepository<InvoiceLineItem> invoiceLineItemRepository,
    IRepository<AppointmentConsent> consentRepository,
    IProfessionalUserLookupService professionalUserLookupService,
    IAdminClinicPatientQueryService adminClinicPatientQueryService,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteAdminClinicVisitCommand, AdminClinicVisitDetailDto?>
{
    public async Task<AdminClinicVisitDetailDto?> Handle(
        CompleteAdminClinicVisitCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.CanDocumentVisitsAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only a doctor or hospital administrator can complete visits.");

        var visit = await visitRepository.GetByIdAsync(request.VisitId, cancellationToken).ConfigureAwait(false);
        if (visit is null || visit.ClinicId != request.ClinicId)
            return null;

        if (visit.Status == "Completed")
            return await MapAsync(visit, cancellationToken).ConfigureAwait(false);

        var completedAt = clock.UtcNow;
        visit.Status = "Completed";
        visit.VisitEnd = completedAt;
        if (request.Summary is not null)
            visit.Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary.Trim();

        var appointment = await appointmentRepository
            .GetByIdAsync(visit.AppointmentId, cancellationToken)
            .ConfigureAwait(false);
        if (appointment is not null && !appointment.IsCancelled)
            appointment.Status = "Completed";

        if (request.BillAmount is decimal billAmount && billAmount > 0)
        {
            var existingInvoice = await invoiceRepository.SearchAsync(
                q => q.Where(i => i.VisitId == visit.Id),
                1,
                1,
                applyDefaultIdOrdering: false,
                cancellationToken).ConfigureAwait(false);
            if (existingInvoice.Items.Count == 0)
            {
                var currency = string.IsNullOrWhiteSpace(request.Currency)
                    ? "ZAR"
                    : request.Currency.Trim().ToUpperInvariant();
                var description = string.IsNullOrWhiteSpace(request.BillDescription)
                    ? "Consultation"
                    : request.BillDescription.Trim();

                var invoice = new Invoice
                {
                    ClinicId = request.ClinicId,
                    PatientId = visit.PatientId,
                    VisitId = visit.Id,
                    Amount = billAmount,
                    Currency = currency,
                    DueDate = completedAt.Date,
                    Status = request.MarkPaid ? "Paid" : "Pending",
                    PaymentMethod = request.MarkPaid ? "Cash" : null,
                    PaidAt = request.MarkPaid ? completedAt : null
                };
                await invoiceRepository.AddAsync(invoice, cancellationToken).ConfigureAwait(false);
                await invoiceLineItemRepository.AddAsync(new InvoiceLineItem
                {
                    InvoiceId = invoice.Id,
                    ServiceType = "Consultation",
                    Description = description,
                    Quantity = 1,
                    UnitPrice = billAmount,
                    TotalPrice = billAmount
                }, cancellationToken).ConfigureAwait(false);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await MapAsync(visit, cancellationToken).ConfigureAwait(false);
    }

    private async Task<AdminClinicVisitDetailDto> MapAsync(Visit visit, CancellationToken cancellationToken)
    {
        var vitalsPage = await vitalRepository.SearchAsync(
            q => q.Where(v => v.VisitId == visit.Id).OrderByDescending(v => v.RecordedAt),
            1,
            100,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var patient = await patientRepository.GetByIdAsync(visit.PatientId, cancellationToken).ConfigureAwait(false);
        var provider = await providerRepository.GetByIdAsync(visit.ProviderId, cancellationToken).ConfigureAwait(false);
        var providerName = "Provider";
        if (provider is not null)
        {
            var users = await professionalUserLookupService
                .GetUsersByIdsAsync([provider.ApplicationUserId], cancellationToken)
                .ConfigureAwait(false);
            var user = users.Count > 0 ? users[0] : null;
            if (user is not null)
                providerName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email : user.DisplayName;
        }

        var clinical = await adminClinicPatientQueryService
            .GetVisitClinicalDocumentationAsync(visit.Id, visit.VisitStart, cancellationToken)
            .ConfigureAwait(false);

        var invoicePage = await invoiceRepository.SearchAsync(
            q => q.Where(i => i.VisitId == visit.Id),
            1,
            1,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);
        var invoice = invoicePage.Items.Count > 0 ? invoicePage.Items[0] : null;

        var consentPage = await consentRepository.SearchAsync(
            q => q.Where(c => c.AppointmentId == visit.AppointmentId),
            1,
            1,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        var consent = consentPage.Items.Count > 0 ? consentPage.Items[0] : null;

        return new AdminClinicVisitDetailDto
        {
            Id = visit.Id,
            ClinicId = visit.ClinicId,
            AppointmentId = visit.AppointmentId,
            PatientId = visit.PatientId,
            PatientName = patient is null ? "Patient" : $"{patient.FirstName} {patient.LastName}".Trim(),
            ProviderId = visit.ProviderId,
            ProviderName = providerName ?? "Provider",
            VisitStart = visit.VisitStart,
            VisitEnd = visit.VisitEnd,
            VisitType = visit.VisitType,
            Status = visit.Status,
            Summary = visit.Summary,
            InvoiceId = invoice?.Id,
            InvoiceAmount = invoice?.Amount,
            InvoiceStatus = invoice?.Status,
            InvoiceCurrency = invoice?.Currency,
            ConsentSigned = consent is not null,
            ConsentSignedAt = consent?.SignedAt,
            Vitals = vitalsPage.Items.Select(v => new AdminClinicVisitVitalDto
            {
                Id = v.Id,
                Type = v.Type,
                Value = v.Value,
                Unit = v.Unit,
                RecordedAt = v.RecordedAt
            }).ToList(),
            Diagnoses = clinical.Diagnoses,
            Prescriptions = clinical.Prescriptions,
            ClinicalNotes = clinical.ClinicalNotes,
            SoapNotes = clinical.SoapNotes,
            LabResults = clinical.LabResults
        };
    }
}
