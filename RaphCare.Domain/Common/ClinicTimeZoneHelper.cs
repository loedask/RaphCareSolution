using System.Globalization;

namespace RaphCare.Domain.Common;

/// <summary>UTC ↔ clinic-local conversion using the hospital's configured time zone id.</summary>
public static class ClinicTimeZoneHelper
{
    public static TimeZoneInfo Resolve(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            return TimeZoneInfo.Utc;

        var id = timeZoneId.Trim();
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException)
        {
            if (TimeZoneInfo.TryConvertIanaIdToWindowsId(id, out var windowsId))
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(windowsId);
                }
                catch (TimeZoneNotFoundException)
                {
                }
            }

            return TimeZoneInfo.Utc;
        }
    }

    public static DateTime ToClinicLocal(DateTime utc, string? timeZoneId)
    {
        var utcDt = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utcDt, Resolve(timeZoneId));
    }

    public static DateTime ToUtcFromClinicLocal(DateTime clinicLocal, string? timeZoneId)
    {
        var unspecified = DateTime.SpecifyKind(clinicLocal, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, Resolve(timeZoneId));
    }

    public static (DateTime fromUtc, DateTime toUtc) GetClinicDayUtcRange(DateTime referenceUtc, string? timeZoneId)
    {
        var local = ToClinicLocal(referenceUtc, timeZoneId);
        var startLocal = local.Date;
        var endLocal = startLocal.AddDays(1).AddTicks(-1);
        return (ToUtcFromClinicLocal(startLocal, timeZoneId), ToUtcFromClinicLocal(endLocal, timeZoneId));
    }

    public static string FormatClinicLocal(DateTime utc, string? timeZoneId, string format = "dd MMM yyyy HH:mm")
        => ToClinicLocal(utc, timeZoneId).ToString(format, CultureInfo.InvariantCulture);
}
