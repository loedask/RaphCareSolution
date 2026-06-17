using MediatR;
using RaphCare.Application.Features.PatientMentalHealth.DTOs;

namespace RaphCare.Application.Features.PatientMentalHealth.Queries.GetMyPatientMentalHealthContent;

/// <summary>Patient hub: load configurable wellness copy (no PHI).</summary>
public sealed class GetMyPatientMentalHealthContentQuery : IRequest<PatientMentalHealthContentDto>;
