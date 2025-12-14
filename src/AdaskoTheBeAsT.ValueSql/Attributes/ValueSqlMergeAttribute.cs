using System;

namespace AdaskoTheBeAsT.ValueSql.Attributes;

/// <summary>
/// Configures SQL Server MERGE statement generation for a TVP class.
/// Apply alongside [ValueSqlTvp] to generate merge extension methods.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ValueSqlMergeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSqlMergeAttribute"/> class.
    /// </summary>
    /// <param name="targetTable">The target table name for the MERGE statement.</param>
    public ValueSqlMergeAttribute(string targetTable)
    {
        TargetTable = targetTable;
    }

    /// <summary>
    /// Gets the target table name for the MERGE statement.
    /// </summary>
    public string TargetTable { get; }

    /// <summary>
    /// Gets or sets the schema of the target table.
    /// Default is "dbo".
    /// </summary>
    public string Schema { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets the columns used in the ON clause for matching rows.
    /// Example: new[] { "Id" } or new[] { "TenantId", "ProductId" } for composite keys.
    /// </summary>
    public string[] MatchColumns { get; set; } = [];

    /// <summary>
    /// Gets or sets the columns to update when a match is found.
    /// If empty, all non-match columns are updated.
    /// </summary>
    public string[] UpdateColumns { get; set; } = [];

    /// <summary>
    /// Gets or sets the columns to insert when no match is found.
    /// If empty, all columns are inserted.
    /// </summary>
    public string[] InsertColumns { get; set; } = [];

    /// <summary>
    /// Gets or sets which MERGE operations to include.
    /// Default is Upsert (Insert + Update).
    /// </summary>
    public MergeOperations Operations { get; set; } = MergeOperations.Upsert;

    /// <summary>
    /// Gets or sets a value indicating whether to output inserted rows.
    /// When true, generates OUTPUT INSERTED.* clause.
    /// </summary>
    public bool OutputInserted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to output deleted rows.
    /// When true, generates OUTPUT DELETED.* clause.
    /// </summary>
    public bool OutputDeleted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to output the action performed.
    /// When true, includes $action in OUTPUT clause ('INSERT', 'UPDATE', 'DELETE').
    /// </summary>
    public bool OutputAction { get; set; }

    /// <summary>
    /// Gets or sets the TVP parameter name in generated SQL.
    /// Default is "@Items".
    /// </summary>
    public string ParameterName { get; set; } = "@Items";

    /// <summary>
    /// Gets or sets the target table alias in MERGE statement.
    /// Default is "t".
    /// </summary>
    public string TargetAlias { get; set; } = "t";

    /// <summary>
    /// Gets or sets the source alias in MERGE statement.
    /// Default is "s".
    /// </summary>
    public string SourceAlias { get; set; } = "s";

    /// <summary>
    /// Gets or sets an optional WHERE clause for the WHEN MATCHED condition.
    /// Example: "t.ModifiedDate &lt; s.ModifiedDate" to only update if source is newer.
    /// </summary>
    public string? UpdateCondition { get; set; }

    /// <summary>
    /// Gets or sets an optional WHERE clause for the WHEN NOT MATCHED BY SOURCE condition.
    /// Example: "t.IsProtected = 0" to only delete unprotected rows.
    /// </summary>
    public string? DeleteCondition { get; set; }
}
