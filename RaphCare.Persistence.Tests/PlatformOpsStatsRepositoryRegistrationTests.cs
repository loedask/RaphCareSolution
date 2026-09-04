using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Devices;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;
using Xunit;

namespace RaphCare.Persistence.Tests;

/// <summary>
/// GET /api/ops/stats constructs GetPlatformOpsStatsHandler. Missing repository
/// registrations surface as a 500 before any count runs.
/// </summary>
public sealed class PlatformOpsStatsRepositoryRegistrationTests
{
    [Theory]
    [InlineData(typeof(IRepository<Clinic>))]
    [InlineData(typeof(IRepository<Patient>))]
    [InlineData(typeof(IRepository<Provider>))]
    [InlineData(typeof(IRepository<ClinicStaffMembership>))]
    [InlineData(typeof(IRepository<Facility>))]
    [InlineData(typeof(IRepository<Device>))]
    [InlineData(typeof(IRepository<Appointment>))]
    [InlineData(typeof(IRepository<InpatientAdmission>))]
    [InlineData(typeof(IRepository<ClinicStaffInvitation>))]
    [InlineData(typeof(IRepository<DeviceEmergencyEvent>))]
    public void AddPersistenceRegistersEachRepositoryUsedByPlatformOpsStats(Type repositoryType)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=(localdb)\\mssqllocaldb;Database=RaphCareOpsStatsRegistration;Trusted_Connection=True;"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddPersistence(configuration);

        Assert.Contains(services, d => d.ServiceType == repositoryType);
    }
}
