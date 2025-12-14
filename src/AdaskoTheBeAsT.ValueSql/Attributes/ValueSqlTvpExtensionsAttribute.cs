using System;

namespace AdaskoTheBeAsT.ValueSql.Attributes;

/// <summary>
/// Generates extension methods on DbConnection for TVP operations.
/// Apply to a class that also has [ValueSqlTvp] attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ValueSqlTvpExtensionsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the SQL for bulk insert operation.
    /// Use @Items as the parameter placeholder.
    /// Example: "INSERT INTO Products SELECT * FROM @Items".
    /// </summary>
    public string? InsertSql { get; set; }

    /// <summary>
    /// Gets or sets the SQL for merge or upsert operation.
    /// Use @Items as the parameter placeholder.
    /// </summary>
    public string? MergeSql { get; set; }

    /// <summary>
    /// Gets or sets the stored procedure name for insert.
    /// </summary>
    public string? InsertStoredProcedure { get; set; }

    /// <summary>
    /// Gets or sets the stored procedure name for merge/upsert.
    /// </summary>
    public string? MergeStoredProcedure { get; set; }

    /// <summary>
    /// Gets or sets the TVP parameter name used in SQL/stored procedures.
    /// Default is "@Items".
    /// </summary>
    public string ParameterName { get; set; } = "@Items";

    /// <summary>
    /// Gets or sets the name of the generated extension class.
    /// Default is "{TypeName}Extensions".
    /// </summary>
    public string? ExtensionsClassName { get; set; }
}
