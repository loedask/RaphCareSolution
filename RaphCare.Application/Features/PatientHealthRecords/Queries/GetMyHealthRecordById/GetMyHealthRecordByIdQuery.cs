using MediatR;
using RaphCare.Application.Features.PatientHealthRecords.DTOs;

namespace RaphCare.Application.Features.PatientHealthRecords.Queries.GetMyHealthRecordById;

public class GetMyHealthRecordByIdQuery : IRequest<PatientHealthRecordDetailDto>
{
    public Guid Id { get; set; }
}
