using MediatR;
using RaphCare.Application.Features.PatientMedicalInfo.DTOs;

namespace RaphCare.Application.Features.PatientMedicalInfo.Queries.GetMyPatientMedicalInfo;

public sealed class GetMyPatientMedicalInfoQuery : IRequest<MyPatientMedicalInfoDto>;
