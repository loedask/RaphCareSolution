using FluentValidation.Results;
using MediatR;
using RaphCare.Application.Common;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Telemedicine;

namespace RaphCare.Application.Features.PatientTelehealth.Commands.RequestOnDemandTelehealthSession;

public sealed class RequestOnDemandTelehealthSessionHandler(
    IRepository<Appointment> appointments,
    IRepository<Visit> visits,
    IRepository<TeleSession> teleSessions,
    IRepository<Provider> providers,
    IRepository<Clinic> clinics,
    IPatientClinicAccessService patientClinics,
    ICurrentUserService currentUser,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<RequestOnDemandTelehealthSessionCommand, Guid>
{
    private readonly IRepository<Appointment> _appointments = appointments;
    private readonly IRepository<Visit> _visits = visits;
    private readonly IRepository<TeleSession> _teleSessions = teleSessions;
    private readonly IRepository<Provider> _providers = providers;
    private readonly IRepository<Clinic> _clinics = clinics;
    private readonly IPatientClinicAccessService _patientClinics = patientClinics;
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IDateTimeProvider _clock = clock;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Guid> Handle(RequestOnDemandTelehealthSessionCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required for telehealth.");

        var clinicId = await ResolveClinicIdAsync(patientId, request.ClinicId, cancellationToken).ConfigureAwait(false);

        var providerId = await ResolveProviderIdAsync(clinicId, request.ProviderId, cancellationToken).ConfigureAwait(false);
        var now = _clock.UtcNow;
        var callMode = string.IsNullOrWhiteSpace(request.CallMode) ? "video" : request.CallMode.Trim().ToLowerInvariant();

        var appointment = new Appointment
        {
            ClinicId = clinicId,
            PatientId = patientId,
            ProviderId = providerId,
            ScheduledStart = now,
            ScheduledEnd = now.AddMinutes(30),
            Type = "Telemedicine",
            Status = "InProgress",
            Reason = $"On-demand {callMode} visit"
        };
        await _appointments.AddAsync(appointment, cancellationToken).ConfigureAwait(false);

        var visit = new Visit
        {
            ClinicId = clinicId,
            AppointmentId = appointment.Id,
            PatientId = patientId,
            ProviderId = providerId,
            VisitStart = now,
            VisitType = "Telemedicine",
            Status = "InProgress",
            Summary = "On-demand telehealth"
        };
        await _visits.AddAsync(visit, cancellationToken).ConfigureAwait(false);

        var teleSession = new TeleSession
        {
            ClinicId = clinicId,
            AppointmentId = appointment.Id,
            VisitId = visit.Id,
            PatientId = patientId,
            ProviderId = providerId,
            ScheduledStart = now,
            ActualStart = now,
            Status = "InProgress",
            Platform = TelehealthPlatforms.Agora,
            SessionExternalId = $"raph-tele-{Guid.NewGuid():N}",
            IsSecure = true
        };
        await _teleSessions.AddAsync(teleSession, cancellationToken).ConfigureAwait(false);

        await _patientClinics.GrantEncounterAccessAsync(patientId, clinicId, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return teleSession.Id;
    }

    private async Task<Guid> ResolveClinicIdAsync(Guid patientId, Guid? requestedClinicId, CancellationToken cancellationToken)
    {
        if (requestedClinicId is { } clinic && clinic != Guid.Empty)
        {
            var exists = await _clinics.GetByIdAsync(clinic, cancellationToken).ConfigureAwait(false);
            if (exists is null || !exists.IsActive)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure(nameof(RequestOnDemandTelehealthSessionCommand.ClinicId),
                        "The selected clinic is not available.")
                });
            }

            return clinic;
        }

        var accessible = await _patientClinics.GetAccessibleClinicIdsAsync(patientId, cancellationToken).ConfigureAwait(false);
        if (accessible.Length > 0)
            return accessible[0];

        var page = await _clinics.SearchAsync(
            q => q.Where(c => c.IsActive),
            pageNumber: 1,
            pageSize: 1,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var first = page.Items.Count > 0 ? page.Items[0] : null;
        if (first is null)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(RequestOnDemandTelehealthSessionCommand.ClinicId),
                    "No clinic is available for on-demand telehealth. Contact support.")
            });
        }

        return first.Id;
    }

    private async Task<Guid> ResolveProviderIdAsync(Guid clinicId, Guid? requestedProviderId, CancellationToken cancellationToken)
    {
        if (requestedProviderId is { } providerId && providerId != Guid.Empty)
        {
            var provider = await _providers.GetByIdAsync(providerId, cancellationToken).ConfigureAwait(false);
            if (provider is null || provider.ClinicId != clinicId || !provider.IsActive || provider.IsDeleted)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure(nameof(RequestOnDemandTelehealthSessionCommand.ProviderId),
                        "The selected provider is not available at this clinic.")
                });
            }

            return providerId;
        }

        var page = await _providers.SearchAsync(
            q => q.Where(p => p.ClinicId == clinicId && p.IsActive && !p.IsDeleted),
            pageNumber: 1,
            pageSize: 1,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var first = page.Items.Count > 0 ? page.Items[0] : null;
        if (first is null)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(RequestOnDemandTelehealthSessionCommand.ProviderId),
                    "No provider is available for on-demand telehealth at this clinic.")
            });
        }

        return first.Id;
    }
}
