namespace RaphCare.Application.Features.Organization;

internal static class ClinicRosterShift
{
    public const string Morning = "Morning";
    public const string Afternoon = "Afternoon";
    public const string Night = "Night";

    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        Morning,
        Afternoon,
        Night
    };

    public static DateTime NormalizeDutyDate(DateTime value) =>
        DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);

    public static string NormalizeLabel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Morning;

        if (value.Equals(Morning, StringComparison.OrdinalIgnoreCase))
            return Morning;
        if (value.Equals(Afternoon, StringComparison.OrdinalIgnoreCase))
            return Afternoon;
        if (value.Equals(Night, StringComparison.OrdinalIgnoreCase))
            return Night;

        return value.Trim();
    }

    public static int SortOrder(string shiftLabel) =>
        shiftLabel switch
        {
            Morning => 0,
            Afternoon => 1,
            Night => 2,
            _ => 9
        };
}
