using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Persistence;

/// <summary>
/// Coordinates SaveChanges across all bounded-context DbContexts so a single request can persist changes from any context.
/// </summary>
public class PersistenceUnitOfWork : IUnitOfWork
{
    private readonly IdentityDbContext _identity;
    private readonly ClinicalDbContext _clinical;
    private readonly DeviceDbContext _device;
    private readonly InsuranceDbContext _insurance;
    private readonly BillingDbContext _billing;
    private readonly AIDbContext _ai;

    public PersistenceUnitOfWork(
        IdentityDbContext identity,
        ClinicalDbContext clinical,
        DeviceDbContext device,
        InsuranceDbContext insurance,
        BillingDbContext billing,
        AIDbContext ai)
    {
        _identity = identity ?? throw new ArgumentNullException(nameof(identity));
        _clinical = clinical ?? throw new ArgumentNullException(nameof(clinical));
        _device = device ?? throw new ArgumentNullException(nameof(device));
        _insurance = insurance ?? throw new ArgumentNullException(nameof(insurance));
        _billing = billing ?? throw new ArgumentNullException(nameof(billing));
        _ai = ai ?? throw new ArgumentNullException(nameof(ai));
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var n = 0;
        n += await _identity.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        n += await _clinical.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        n += await _device.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        n += await _insurance.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        n += await _billing.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        n += await _ai.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return n;
    }
}
