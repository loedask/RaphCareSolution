namespace RaphCare.Mobile.Core.Common.Services.Api;

/// <summary>
/// Picks the first non-empty clinic Guid from candidates (selected store, then config fallbacks).
/// </summary>
public static class ClinicIdResolution
{
    public static Guid? Resolve(params Guid?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (candidate is { } id && id != Guid.Empty)
                return id;
        }

        return null;
    }
}
