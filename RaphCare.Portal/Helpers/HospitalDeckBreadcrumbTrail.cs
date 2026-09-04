using System.Globalization;
using RaphCare.Portal.Resources.Strings;

namespace RaphCare.Portal.Helpers;

/// <summary>
/// Builds the All hospitals → clinic → section → leaf path for the deck crumbtrail.
/// </summary>
public static class HospitalDeckBreadcrumbTrail
{
    public static IReadOnlyList<HospitalDeckCrumb> Build(
        Guid clinicId,
        string clinicName,
        string? current = null,
        string? midLabel = null,
        string? midHref = null)
    {
        var culture = CultureInfo.CurrentUICulture;
        var name = string.IsNullOrWhiteSpace(clinicName)
            ? AppResources.T("Common_ThisHospital", culture)
            : clinicName.Trim();

        var hasMid = !string.IsNullOrWhiteSpace(midLabel);
        var hasCurrent = !string.IsNullOrWhiteSpace(current);
        var clinicHref = hasMid || hasCurrent
            ? $"/admin/hospitals/{clinicId}"
            : null;

        var crumbs = new List<HospitalDeckCrumb>(4)
        {
            new(AppResources.T("AdminLayout_AllHospitals", culture), "/admin/hospitals"),
            new(name, clinicHref)
        };

        if (hasMid)
        {
            crumbs.Add(new(
                midLabel!.Trim(),
                hasCurrent
                    ? (string.IsNullOrWhiteSpace(midHref) ? clinicHref : midHref.Trim())
                    : null));
        }

        if (hasCurrent)
            crumbs.Add(new(current!.Trim()));

        return crumbs;
    }
}
