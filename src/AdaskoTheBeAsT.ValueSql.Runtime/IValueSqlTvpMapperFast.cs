using System.Collections.Generic;
using Microsoft.Data.SqlClient.Server;

namespace AdaskoTheBeAsT.ValueSql.Runtime;

/// <summary>
/// High-performance interface for Table-Valued Parameter mapping using SqlDataRecord.
/// Generates streaming records without buffering entire collection.
/// </summary>
/// <typeparam name="T">The entity type to map.</typeparam>
public interface IValueSqlTvpMapperFast<in T>
{
    /// <summary>
    /// Gets the SQL Server type name for this TVP (e.g., "dbo.ProductTableType").
    /// </summary>
    string SqlTypeName { get; }

    /// <summary>
    /// Gets the SqlMetaData array describing the TVP columns.
    /// Cached statically for zero-allocation reuse.
    /// </summary>
    SqlMetaData[] GetMetaData();

    /// <summary>
    /// Converts a collection to IEnumerable&lt;SqlDataRecord&gt; for TVP parameter.
    /// Streams records one at a time - no buffering, minimal allocations.
    /// </summary>
    /// <example>
    /// <code>
    /// var mapper = default(ProductTvpMapper);
    /// var parameter = new SqlParameter("@products", SqlDbType.Structured)
    /// {
    ///     TypeName = mapper.SqlTypeName,
    ///     Value = mapper.ToSqlDataRecords(products)
    /// };
    /// command.Parameters.Add(parameter);
    /// </code>
    /// </example>
    IEnumerable<SqlDataRecord> ToSqlDataRecords(IEnumerable<T> items);
}
