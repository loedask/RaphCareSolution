using MediatR;
using RaphCare.Application.Features.PatientNotifications.DTOs;

namespace RaphCare.Application.Features.PatientNotifications.Queries.GetMyPatientNotifications;

/// <summary>Lists in-app notifications for the JWT <c>patientId</c> claim, newest first.</summary>
public sealed class GetMyPatientNotificationsQuery : IRequest<IReadOnlyList<PatientNotificationDto>>;
