namespace RaphCare.Domain.Billing;

/// <summary>Canonical SKU codes for the commercial price catalog (price list v2).</summary>
public static class PriceCatalogSku
{
    public const string SitePractice = "SITE_PRACTICE";
    public const string SiteClinic = "SITE_CLINIC";
    public const string SiteHospital = "SITE_HOSPITAL";
    public const string SiteNetwork = "SITE_NETWORK";
    public const string SeatExtra = "SEAT_EXTRA";
    public const string CareEssential = "CARE_ESSENTIAL";
    public const string CareComplete = "CARE_COMPLETE";
    public const string PkgHealthTrack = "PKG_HEALTH_TRACK";
    public const string PkgSafeCare = "PKG_SAFECARE";
    public const string UsageVideoExtra = "USAGE_VIDEO_EXTRA";
    public const string AddonClinicAi = "ADDON_CLINIC_AI";

    public const int SkuCodeMaxLength = 64;
    public const int DisplayNameMaxLength = 128;
    public const int CategoryMaxLength = 32;

    public static class Categories
    {
        public const string Site = "Site";
        public const string Seat = "Seat";
        public const string PatientCare = "PatientCare";
        public const string WatchPackage = "WatchPackage";
        public const string Usage = "Usage";
        public const string Addon = "Addon";
    }
}
