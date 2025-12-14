using System;

namespace AdaskoTheBeAsT.ValueSql.Attributes;

/// <summary>
/// Marks a partial method in a [ValueSqlRepository] class for implementation generation.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ValueSqlMethodAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSqlMethodAttribute"/> class.
    /// </summary>
    /// <param name="sql">The SQL command or stored procedure name to execute.</param>
    public ValueSqlMethodAttribute(string sql)
    {
        Sql = sql;
    }

    /// <summary>
    /// Gets the SQL command or stored procedure name.
    /// </summary>
    public string Sql { get; }

    /// <summary>
    /// Gets or sets a value indicating whether Sql is a stored procedure name.
    /// Default is false (raw SQL).
    /// </summary>
    public bool IsStoredProcedure { get; set; }

    /// <summary>
    /// Gets or sets the TVP parameter name when the method has an IEnumerable parameter.
    /// Default is "@Items".
    /// </summary>
    public string TvpParameterName { get; set; } = "@Items";

    /// <summary>
    /// Gets or sets the command timeout in seconds.
    /// Default is 30.
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
}
