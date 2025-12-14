using System;

namespace AdaskoTheBeAsT.ValueSql.Attributes;

/// <summary>
/// Specifies which operations to include in a MERGE statement.
/// </summary>
[Flags]
public enum MergeOperations
{
    /// <summary>
    /// No operations (invalid for MERGE).
    /// </summary>
    None = 0,

    /// <summary>
    /// Include WHEN NOT MATCHED BY TARGET THEN INSERT clause.
    /// </summary>
    Insert = 1,

    /// <summary>
    /// Include WHEN MATCHED THEN UPDATE clause.
    /// </summary>
    Update = 2,

    /// <summary>
    /// Include WHEN NOT MATCHED BY SOURCE THEN DELETE clause.
    /// </summary>
    Delete = 4,

    /// <summary>
    /// Upsert: Insert new rows, update existing (Insert + Update).
    /// </summary>
    Upsert = Insert | Update,

    /// <summary>
    /// Full sync: Insert, update, and delete to match source (Insert + Update + Delete).
    /// </summary>
    Sync = Insert | Update | Delete,
}
