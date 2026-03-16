using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace RaphCare.Persistence.Utilities;

/// <summary>
/// SQL Server–specific detection of unique constraint violations from <see cref="DbUpdateException"/>.
/// Error codes: 2601 (unique index), 2627 (UNIQUE KEY constraint).
/// </summary>
public static class SqlServerExceptionDetector
{
    /// <summary>
    /// Returns true if the exception was caused by a SQL Server unique constraint or unique index violation.
    /// </summary>
    public static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        if (ex.InnerException is SqlException sqlEx)
        {
            return sqlEx.Number == 2601 || sqlEx.Number == 2627;
        }

        if (ex.InnerException?.InnerException is SqlException innerSql)
        {
            return innerSql.Number == 2601 || innerSql.Number == 2627;
        }

        return false;
    }
}
