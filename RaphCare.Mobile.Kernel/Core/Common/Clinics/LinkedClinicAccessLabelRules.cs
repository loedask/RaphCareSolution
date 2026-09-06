namespace RaphCare.Mobile.Core.Common.Clinics;

/// <summary>Maps API access-kind codes to AppResources keys for the linked-hospitals list.</summary>
public static class LinkedClinicAccessLabelRules
{
    public static string ResourceKey(string? accessKind) => accessKind?.Trim() switch
    {
        "Registered" => "SelectClinicAccessRegistered",
        "ManualGrant" => "SelectClinicAccessManual",
        "EncounterBased" => "SelectClinicAccessEncounter",
        "InsuranceLinked" => "SelectClinicAccessInsurance",
        "CareHistory" => "SelectClinicAccessCareHistory",
        _ => "SelectClinicAccessCareHistory",
    };
}
