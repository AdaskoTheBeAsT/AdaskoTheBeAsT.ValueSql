using System.Data;

namespace AdaskoTheBeAsT.ValueSql.Runtime;

/// <summary>
/// Configuration options for ValueSql query execution.
/// </summary>
public sealed class ValueSqlOptions
{
    /// <summary>
    /// Gets the default options instance.
    /// </summary>
    public static ValueSqlOptions Default { get; } = new();

    /// <summary>
    /// Gets options optimized for streaming large result sets with BLOBs.
    /// Uses SequentialAccess for reduced memory pressure.
    /// </summary>
    public static ValueSqlOptions Streaming { get; } = new()
    {
        UseSequentialAccess = true,
    };

    /// <summary>
    /// Gets options optimized for single-row queries.
    /// Uses SingleResult and SingleRow hints.
    /// </summary>
    public static ValueSqlOptions SingleRow { get; } = new()
    {
        UseSingleResult = true,
        UseSingleRow = true,
    };

    /// <summary>
    /// Gets or sets whether to use CommandBehavior.SequentialAccess.
    /// Default: false (adds overhead for small result sets).
    /// Set to true only for large BLOBs or streaming scenarios.
    /// </summary>
    public bool UseSequentialAccess { get; set; }

    /// <summary>
    /// Gets or sets whether to use CommandBehavior.SingleResult.
    /// Default: false. Set to true when query returns only one result set.
    /// </summary>
    public bool UseSingleResult { get; set; }

    /// <summary>
    /// Gets or sets whether to use CommandBehavior.SingleRow.
    /// Default: false. Set to true when expecting a single row.
    /// </summary>
    public bool UseSingleRow { get; set; }

    /// <summary>
    /// Gets or sets whether to close the connection after reading.
    /// Default: false (caller manages connection lifetime).
    /// </summary>
    public bool CloseConnection { get; set; }

    /// <summary>
    /// Gets or sets the estimated row count for pre-allocating collections.
    /// Default: 0 (no pre-allocation hint).
    /// </summary>
    public int EstimatedRowCount { get; set; }

    /// <summary>
    /// Builds the CommandBehavior flags based on current options.
    /// </summary>
    public CommandBehavior ToCommandBehavior()
    {
        var behavior = CommandBehavior.Default;

        if (UseSequentialAccess)
        {
            behavior |= CommandBehavior.SequentialAccess;
        }

        if (UseSingleResult)
        {
            behavior |= CommandBehavior.SingleResult;
        }

        if (UseSingleRow)
        {
            behavior |= CommandBehavior.SingleRow;
        }

        if (CloseConnection)
        {
            behavior |= CommandBehavior.CloseConnection;
        }

        return behavior;
    }
}
