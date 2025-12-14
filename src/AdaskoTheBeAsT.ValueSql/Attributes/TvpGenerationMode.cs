namespace AdaskoTheBeAsT.ValueSql.Attributes;

/// <summary>
/// Specifies how Table-Valued Parameter mapping code should be generated.
/// </summary>
public enum TvpGenerationMode
{
    /// <summary>
    /// Generate IEnumerable&lt;SqlDataRecord&gt; - fastest approach, recommended for most scenarios.
    /// Uses SqlMetaData for column definitions and streams records without buffering.
    /// Best for: Stored procedure parameters, bulk operations.
    /// </summary>
    SqlDataRecord = 0,

    /// <summary>
    /// Generate DbDataReader-based TVP using custom IDataReader implementation.
    /// Useful for scenarios requiring IDataReader interface compatibility.
    /// Streams data without buffering, similar performance to SqlDataRecord.
    /// </summary>
    DbDataReader = 1,

    /// <summary>
    /// Generate both SqlDataRecord and DbDataReader implementations.
    /// Useful when you need flexibility for different consumers.
    /// </summary>
    Both = 2,
}
