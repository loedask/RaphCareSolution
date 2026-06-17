using RaphCare.Domain.Identity;

namespace RaphCare.Application.Common.Interfaces;

public interface ITokenService
{
    string GeneratePatientToken(ApplicationUser user, Guid patientId);
}
