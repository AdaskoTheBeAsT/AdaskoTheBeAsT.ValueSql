using Microsoft.Data.SqlClient;

namespace AdaskoTheBeAsT.ValueSql.Runtime;

/// <summary>
/// High-performance mapper interface that works directly with SqlDataReader
/// to avoid virtual dispatch overhead of IDataReader.
/// </summary>
/// <typeparam name="T">The entity type to map.</typeparam>
public interface IValueSqlMapperFast<out T>
{
    /// <summary>
    /// Maps the current row to an entity using direct SqlDataReader access.
    /// Generated implementations use AggressiveInlining and SkipLocalsInit.
    /// </summary>
    T MapFast(SqlDataReader reader);
}
