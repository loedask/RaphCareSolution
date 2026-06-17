using MediatR;
using RaphCare.Application.Features.PatientProfile.DTOs;

namespace RaphCare.Application.Features.PatientProfile.Queries.GetMyPatientProfile;

/// <summary>Returns the current patient’s demographics from the clinical record.</summary>
public sealed class GetMyPatientProfileQuery : IRequest<MyPatientProfileDto>;
