using System;

namespace AdaskoTheBeAsT.ValueSql.Attributes;

/// <summary>
/// Specifies column metadata for TVP and query mapping.
/// Use to configure SQL type details like string length, decimal precision, etc.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ValueSqlColumnAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSqlColumnAttribute"/> class.
    /// </summary>
    public ValueSqlColumnAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSqlColumnAttribute"/> class with the specified column name.
    /// </summary>
    /// <param name="name">The column name in the database.</param>
    public ValueSqlColumnAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets or sets the column name in the database (defaults to property name).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the maximum length for string/binary columns.
    /// Use -1 for MAX (nvarchar(max), varbinary(max)).
    /// Default: 4000 for strings, -1 for byte[].
    /// </summary>
    public int Length { get; set; } = -2; // -2 means "use default"

    /// <summary>
    /// Gets or sets the precision for decimal columns (total digits).
    /// Default: 18.
    /// </summary>
    public byte Precision { get; set; } = 18;

    /// <summary>
    /// Gets or sets the scale for decimal columns (digits after decimal point).
    /// Default: 4.
    /// </summary>
    public byte Scale { get; set; } = 4;

    /// <summary>
    /// Gets or sets the explicit SQL type to use (overrides auto-detection).
    /// Example: "date", "datetime2", "varchar", "nchar".
    /// </summary>
    public string? SqlType { get; set; }

    /// <summary>
    /// Gets or sets the precision for datetime2, datetimeoffset, and time columns (0-7).
    /// Default: 7 (100 nanoseconds).
    /// Use 3 for milliseconds, 0 for seconds only.
    /// </summary>
    public byte DateTimePrecision { get; set; } = 7;
}
