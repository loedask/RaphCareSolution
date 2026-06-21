namespace RaphCare.Mobile.Core.Features.Settings.Services;

/// <summary>Device-local cache for profile demographics (synced after API success) and privacy toggles.</summary>
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

    string Address { get; set; }
    string BloodType { get; set; }
    string Allergies { get; set; }
    string ChronicConditions { get; set; }
    string Medications { get; set; }
    string PrimaryDoctor { get; set; }

    IReadOnlyList<Models.StoredEmergencyContact> GetEmergencyContacts();
    void SetEmergencyContacts(IReadOnlyList<Models.StoredEmergencyContact> contacts);
}
