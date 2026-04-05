namespace RaphCare.Mobile.Core.Features.Settings.Services;

/// <summary>Device-local patient profile and privacy toggles until a dedicated profile API ships (Vertical 12).</summary>
public interface ILocalPatientProfileStore
{
    string FirstName { get; set; }
    string LastName { get; set; }
    string Email { get; set; }
    string Phone { get; set; }
    DateTime? DateOfBirth { get; set; }
    string Gender { get; set; }

    bool PrivacyDataSharing { get; set; }
    bool PrivacyTwoFactor { get; set; }
}
