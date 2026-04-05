using MediatR;
using RaphCare.Application.Features.PatientBilling.DTOs;

namespace RaphCare.Application.Features.PatientBilling.Queries.GetMyPaymentMethods;

public sealed class GetMyPaymentMethodsQuery : IRequest<IReadOnlyList<PatientPaymentMethodDto>>;
