namespace RaphCare.Mobile.Core.Common.Home;

/// <summary>Picks the next upcoming appointment for the Home card (not demo sample data).</summary>
public static class HomeUpcomingAppointmentRules
{
    /// <summary>
    /// Returns the soonest appointment that has not ended yet, ignoring cancelled rows.
    /// </summary>
    public static TAppointment? PickNextUpcoming<TAppointment>(
        IEnumerable<TAppointment> appointments,
        DateTime utcNow,
        Func<TAppointment, DateTime> scheduledStart,
        Func<TAppointment, DateTime> scheduledEnd,
        Func<TAppointment, string?> status)
    {
        ArgumentNullException.ThrowIfNull(appointments);
        ArgumentNullException.ThrowIfNull(scheduledStart);
        ArgumentNullException.ThrowIfNull(scheduledEnd);
        ArgumentNullException.ThrowIfNull(status);

        return appointments
            .Where(a =>
            {
                var s = status(a);
                if (!string.IsNullOrWhiteSpace(s)
                    && s.Contains("cancel", StringComparison.OrdinalIgnoreCase))
                    return false;
                return scheduledEnd(a) >= utcNow;
            })
            .OrderBy(scheduledStart)
            .FirstOrDefault();
    }

    /// <summary>True when the appointment type looks like a video or telehealth visit.</summary>
    public static bool LooksLikeVideo(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return false;
        return type.Contains("video", StringComparison.OrdinalIgnoreCase)
               || type.Contains("tele", StringComparison.OrdinalIgnoreCase);
    }

    public static string InitialsFromLabel(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
            return "AP";

        var parts = label.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return string.Concat(parts[0][0], parts[1][0]).ToUpperInvariant();
        return parts[0].Length >= 2
            ? parts[0][..2].ToUpperInvariant()
            : parts[0].ToUpperInvariant();
    }
}
