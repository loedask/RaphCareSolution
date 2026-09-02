using RaphCare.Client.Models.Clinical;
using RaphCare.Domain.Devices;

namespace RaphCare.Web.Helpers;

/// <summary>Picks SOS and fall alerts for the hospital home boards.</summary>
public static class ClinicEmergencyHomeFilter
{
    public const int DefaultTake = 8;
    public static readonly TimeSpan RecentWindow = TimeSpan.FromHours(72);

    public static bool IsHomePriority(string? eventType) =>
        string.Equals(eventType, StandaloneEmergencyEventTypes.Sos, StringComparison.OrdinalIgnoreCase)
        || string.Equals(eventType, StandaloneEmergencyEventTypes.Fall, StringComparison.OrdinalIgnoreCase);

    public static IReadOnlyList<ClinicDeviceEmergencyEventViewModel> TakeHomePriority(
        IEnumerable<ClinicDeviceEmergencyEventViewModel> source,
        int take = DefaultTake) =>
        source
            .Where(ev => IsHomePriority(ev.EventType))
            .OrderByDescending(ev => ev.OccurredAtUtc)
            .Take(take)
            .ToList();
}
