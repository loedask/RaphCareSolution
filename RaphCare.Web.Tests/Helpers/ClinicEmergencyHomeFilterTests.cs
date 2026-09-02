using RaphCare.Client.Models.Clinical;
using RaphCare.Domain.Devices;
using RaphCare.Web.Helpers;
using Xunit;

namespace RaphCare.Web.Tests.Helpers;

public sealed class ClinicEmergencyHomeFilterTests
{
    [Fact]
    public void TakeHomePriorityKeepsSosAndFallAndDropsLocationNoise()
    {
        var now = new DateTime(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc);
        var items = new[]
        {
            new ClinicDeviceEmergencyEventViewModel
            {
                EventType = StandaloneEmergencyEventTypes.LocationPing,
                OccurredAtUtc = now,
                PatientName = "Noise"
            },
            new ClinicDeviceEmergencyEventViewModel
            {
                EventType = StandaloneEmergencyEventTypes.Fall,
                OccurredAtUtc = now.AddMinutes(-10),
                PatientName = "Fall patient"
            },
            new ClinicDeviceEmergencyEventViewModel
            {
                EventType = StandaloneEmergencyEventTypes.Sos,
                OccurredAtUtc = now.AddMinutes(-5),
                PatientName = "SOS patient"
            },
            new ClinicDeviceEmergencyEventViewModel
            {
                EventType = StandaloneEmergencyEventTypes.LowBattery,
                OccurredAtUtc = now.AddMinutes(-1),
                PatientName = "Battery"
            }
        };

        var home = ClinicEmergencyHomeFilter.TakeHomePriority(items, take: 8);

        Assert.Equal(2, home.Count);
        Assert.Equal("SOS patient", home[0].PatientName);
        Assert.Equal("Fall patient", home[1].PatientName);
    }

    [Fact]
    public void IsHomePriorityRejectsUnknownTypes()
    {
        Assert.True(ClinicEmergencyHomeFilter.IsHomePriority("sos"));
        Assert.True(ClinicEmergencyHomeFilter.IsHomePriority("Fall"));
        Assert.False(ClinicEmergencyHomeFilter.IsHomePriority(StandaloneEmergencyEventTypes.LocationPing));
        Assert.False(ClinicEmergencyHomeFilter.IsHomePriority(null));
    }
}
