using MediatR;
using RaphCare.Application.Features.PatientClinics.DTOs;

namespace RaphCare.Application.Features.PatientClinics.Queries.GetMyLinkedClinics;

public sealed class GetMyLinkedClinicsQuery : IRequest<IReadOnlyList<PatientLinkedClinicDto>>;
