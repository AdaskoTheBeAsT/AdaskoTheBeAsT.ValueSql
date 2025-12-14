using System.Collections.Generic;
using System.Data;

namespace AdaskoTheBeAsT.ValueSql.Abstractions;

/// <summary>
/// Interface for Table-Valued Parameter mapping using DbDataReader approach.
/// Provides streaming IDataReader implementation for TVP parameters.
/// </summary>
/// <typeparam name="T">The entity type to map.</typeparam>
public interface IValueSqlTvpMapper<in T>
{
    /// <summary>
    /// Gets the SQL Server type name for this TVP (e.g., "dbo.ProductTableType").
    /// </summary>
    string SqlTypeName { get; }

    /// <summary>
    /// Creates a streaming IDataReader for the collection.
    /// Does not buffer data - streams items on demand.
    /// </summary>
    IDataReader ToDataReader(IEnumerable<T> items);
}
