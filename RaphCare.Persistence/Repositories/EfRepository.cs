using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Common.DTOs;
using System.Linq;

namespace RaphCare.Persistence.Repositories;

/// <summary>
/// Generic EF Core repository for a given entity and DbContext. Used by Application handlers.
/// </summary>
public class EfRepository<TEntity, TContext> : IRepository<TEntity>
    where TEntity : class
    where TContext : DbContext
{
    private readonly TContext _context;

    public EfRepository(TContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a server-side EF query with optional filtering and minimal paging.
    /// </summary>
    public virtual async Task<PagedResult<TEntity>> SearchAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper,
        int pageNumber,
        int pageSize,
        bool applyDefaultIdOrdering = true,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
            pageNumber = 1;
        if (pageSize < 1)
            pageSize = 1;

        IQueryable<TEntity> query = _context.Set<TEntity>().AsNoTracking();

        if (queryShaper is not null)
            query = queryShaper(query);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var skip = (pageNumber - 1) * pageSize;

        if (applyDefaultIdOrdering)
            query = query.OrderBy(e => EF.Property<Guid>(e, "Id"));

        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<TEntity>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <inheritdoc />
    public virtual Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Add(entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Update(entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }
}
