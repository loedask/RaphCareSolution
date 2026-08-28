using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that commits the unit of work (SaveChanges) after the handler completes successfully.
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Creates the transaction behavior.</summary>
    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return response;
    }
}

