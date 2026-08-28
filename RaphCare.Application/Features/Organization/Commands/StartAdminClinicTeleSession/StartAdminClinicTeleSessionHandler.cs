using System.Buffers.Binary;
using MediatR;
using RaphCare.Application.Common;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;
using RaphCare.Domain.Telemedicine;

namespace RaphCare.Application.Features.Organization.Commands.StartAdminClinicTeleSession;

public sealed class StartAdminClinicTeleSessionHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Appointment> appointmentRepository,
    IRepository<Visit> visitRepository,
    IRepository<TeleSession> teleSessionRepository,
    IRepository<Provider> providerRepository,
    IRepository<Patient> patientRepository,
    IProfessionalUserLookupService professionalUserLookupService,
    ITelehealthRtcTokenGenerator rtcTokenGenerator,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<StartAdminClinicTeleSessionCommand, AdminClinicTeleJoinInfoDto?>
{
    private const int DefaultTtlSeconds = 3600;

    public async Task<AdminClinicTeleJoinInfoDto?> Handle(
        StartAdminClinicTeleSessionCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can start telehealth sessions.");

        var appointment = await appointmentRepository
            .GetByIdAsync(request.AppointmentId, cancellationToken)
            .ConfigureAwait(false);

        if (appointment is null || appointment.ClinicId != request.ClinicId)
            return null;

        if (appointment.IsCancelled)
            throw new BusinessRuleException("Cannot start telehealth for a cancelled appointment.");

        if (!string.Equals(appointment.Type, "Telemedicine", StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Telehealth is only available for telemedicine appointments.");

        var now = clock.UtcNow;

        var openVisitPage = await visitRepository.SearchAsync(
            q => q.Where(v =>
                v.AppointmentId == appointment.Id
                && v.Status != "Completed"
                && v.Status != "Cancelled"),
            1,
            1,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        Visit visit;
        if (openVisitPage.Items.Count > 0)
        {
            visit = openVisitPage.Items[0];
        }
        else
        {
            visit = new Visit
            {
                ClinicId = appointment.ClinicId,
                AppointmentId = appointment.Id,
                PatientId = appointment.PatientId,
                ProviderId = appointment.ProviderId,
                VisitStart = now,
                VisitType = "Telemedicine",
                Status = "InProgress",
                Summary = "Telehealth visit"
            };
            await visitRepository.AddAsync(visit, cancellationToken).ConfigureAwait(false);
        }

        var existingSessionPage = await teleSessionRepository.SearchAsync(
            q => q.Where(s => s.AppointmentId == appointment.Id && s.Status != "Cancelled"),
            1,
            1,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        TeleSession session;
        if (existingSessionPage.Items.Count > 0)
        {
            session = existingSessionPage.Items[0];
            if (session.Status is "Scheduled" or "InProgress")
            {
                session.Status = "InProgress";
                session.ActualStart ??= now;
                session.VisitId = visit.Id;
            }
        }
        else
        {
            session = new TeleSession
            {
                ClinicId = appointment.ClinicId,
                AppointmentId = appointment.Id,
                VisitId = visit.Id,
                PatientId = appointment.PatientId,
                ProviderId = appointment.ProviderId,
                ScheduledStart = appointment.ScheduledStart,
                ActualStart = now,
                Status = "InProgress",
                Platform = TelehealthPlatforms.Agora,
                SessionExternalId = $"raph-tele-{Guid.NewGuid():N}",
                IsSecure = true
            };
            await teleSessionRepository.AddAsync(session, cancellationToken).ConfigureAwait(false);
        }

        if (appointment.Status is "Scheduled" or "InProgress")
            appointment.Status = "InProgress";

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var provider = await providerRepository.GetByIdAsync(appointment.ProviderId, cancellationToken).ConfigureAwait(false);
        var uidSource = provider?.ApplicationUserId
            ?? currentUserService.CurrentUserId
            ?? appointment.ProviderId;
        var uid = StableUidFromGuid(uidSource);

        var channel = string.IsNullOrWhiteSpace(session.SessionExternalId)
            ? $"raph-tele-{session.Id:N}"
            : session.SessionExternalId.Trim();

        var patient = await patientRepository.GetByIdAsync(appointment.PatientId, cancellationToken).ConfigureAwait(false);
        var patientName = patient is null ? "Patient" : $"{patient.FirstName} {patient.LastName}".Trim();

        string? providerName = null;
        if (provider is not null)
        {
            var users = await professionalUserLookupService
                .GetUsersByIdsAsync([provider.ApplicationUserId], cancellationToken)
                .ConfigureAwait(false);
            var user = users.Count > 0 ? users[0] : null;
            if (user is not null)
                providerName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email : user.DisplayName;
        }

        var dto = new AdminClinicTeleJoinInfoDto
        {
            TeleSessionId = session.Id,
            VisitId = visit.Id,
            AppointmentId = appointment.Id,
            ChannelName = channel,
            Uid = uid,
            AppId = rtcTokenGenerator.AppId,
            Status = session.Status,
            ScheduledStart = session.ScheduledStart,
            RtcConfigured = rtcTokenGenerator.IsConfigured,
            TokenExpiresAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + DefaultTtlSeconds,
            PatientName = patientName,
            ProviderDisplayName = providerName
        };

        if (rtcTokenGenerator.IsConfigured)
            dto.RtcToken = rtcTokenGenerator.BuildRtcToken(channel, uid, DefaultTtlSeconds);

        return dto;
    }

    private static uint StableUidFromGuid(Guid g)
    {
        Span<byte> bytes = stackalloc byte[16];
        g.TryWriteBytes(bytes);
        var n = BinaryPrimitives.ReadUInt32LittleEndian(bytes);
        return n == 0 ? 1u : n & 0x7FFF_FFFFu;
    }
}
