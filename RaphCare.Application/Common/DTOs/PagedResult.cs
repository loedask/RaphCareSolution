namespace RaphCare.Application.Common.DTOs;

/// <summary>
/// Application-layer DTO for a single page of list results (e.g. patients, visits, invoices).
/// </summary>
/// <typeparam name="T">Item type (e.g. PatientDto, VisitDto).</typeparam>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

