namespace RaphCare.Client.Models.Api;

/// <summary>Wire shape for paginated API responses.</summary>
public sealed class PagedApiResult<T>
{
    public IReadOnlyList<T>? Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
