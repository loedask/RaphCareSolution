using Microsoft.EntityFrameworkCore;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Detects whether a <see cref="DbUpdateException"/> represents a unique constraint violation.
/// Implementations are database-specific (e.g. SQL Server); the application layer remains database-agnostic.
/// </summary>
public interface IUniqueConstraintViolationDetector
{
    /// <summary>
    /// Returns true if the exception was caused by a unique key / unique index violation.
    /// </summary>
    bool IsUniqueConstraintViolation(DbUpdateException ex);
}
