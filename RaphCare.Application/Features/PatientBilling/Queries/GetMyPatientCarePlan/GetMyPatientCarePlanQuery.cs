using MediatR;
using RaphCare.Application.Features.PatientBilling.DTOs;

namespace RaphCare.Application.Features.PatientBilling.Queries.GetMyPatientCarePlan;

public sealed class GetMyPatientCarePlanQuery : IRequest<PatientCarePlanDto>;
