using RaphCare.Application.Features.PatientSupport.DTOs;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>Supplies FAQ and contact details for patient help (<c>api/patient/support</c>).</summary>
public interface IPatientSupportContentProvider
{
    Task<PatientSupportContentDto> GetContentAsync(CancellationToken cancellationToken = default);
}
