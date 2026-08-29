using RaphCare.Client.Contracts;
using RaphCare.Client.Models;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Anonymous waiting-room board. Pickup codes only.</summary>
public interface ICollectionDisplayService
{
    Task<Response<CollectionDisplayBoard>> GetBoardAsync(string token, CancellationToken cancellationToken = default);
}
