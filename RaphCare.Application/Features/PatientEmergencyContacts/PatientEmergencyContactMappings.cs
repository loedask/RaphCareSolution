using RaphCare.Application.Features.PatientEmergencyContacts.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientEmergencyContacts;

internal static class PatientEmergencyContactMappings
{
    public static PatientEmergencyContactDto ToDto(EmergencyContact e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Relationship = e.Relationship,
        PhoneNumber = e.PhoneNumber,
        Email = e.Email,
    };
}
