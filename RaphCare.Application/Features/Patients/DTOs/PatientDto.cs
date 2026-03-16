using RaphCare.Application.Common.DTOs;

namespace RaphCare.Application.Features.Patients.DTOs;

public class PatientDto : BaseDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

