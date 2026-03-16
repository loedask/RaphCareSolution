using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Persistence.Utilities;

/// <summary>
/// Application-layer implementation of unique constraint detection using SQL Server–specific logic.
/// </summary>
public sealed class SqlServerUniqueConstraintViolationDetector : IUniqueConstraintViolationDetector
{
    /// <inheritdoc />
    public bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        SqlServerExceptionDetector.IsUniqueConstraintViolation(ex);
}
