using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetCollectionDisplay;

public sealed class GetCollectionDisplayHandler(IAdminClinicCollectionQueryService collectionQueryService)
    : IRequestHandler<GetCollectionDisplayQuery, CollectionDisplayBoardDto?>
{
    public Task<CollectionDisplayBoardDto?> Handle(
        GetCollectionDisplayQuery request,
        CancellationToken cancellationToken) =>
        collectionQueryService.GetDisplayBoardAsync(request.Token, cancellationToken);
}
