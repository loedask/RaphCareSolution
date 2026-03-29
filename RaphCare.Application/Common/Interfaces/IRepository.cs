using System.Linq;
using RaphCare.Application.Common.DTOs;

namespace RaphCare.Application.Common.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Executes an EF query with optional filtering, then returns total count and a single page of items.
    /// </summary>
    /// <param name="applyDefaultIdOrdering">When true (default), results are ordered by entity <c>Id</c> before paging. Set false when <paramref name="queryShaper"/> already applies a stable order (e.g. appointment date).</param>
    Task<PagedResult<T>> SearchAsync(
        Func<IQueryable<T>, IQueryable<T>>? queryShaper,
        int pageNumber,
        int pageSize,
        bool applyDefaultIdOrdering = true,
        CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}

