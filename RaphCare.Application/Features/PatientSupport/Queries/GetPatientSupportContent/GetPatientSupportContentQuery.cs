using MediatR;
using RaphCare.Application.Features.PatientSupport.DTOs;

namespace RaphCare.Application.Features.PatientSupport.Queries.GetPatientSupportContent;

public sealed class GetPatientSupportContentQuery : IRequest<PatientSupportContentDto>;
