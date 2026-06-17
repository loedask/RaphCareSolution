using RaphCare.Application.Features.PatientFamilyMembers.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientFamilyMembers;

internal static class PatientFamilyMemberMappings
{
    public static PatientFamilyMemberDto ToDto(PatientFamilyMember e) => new()
    {
        Id = e.Id,
        FirstName = e.FirstName,
        LastName = e.LastName,
        Relationship = e.Relationship,
        DateOfBirth = e.DateOfBirth,
        PhoneNumber = e.PhoneNumber,
        Email = e.Email,
        LinkedPatientId = e.LinkedPatientId
    };
}
