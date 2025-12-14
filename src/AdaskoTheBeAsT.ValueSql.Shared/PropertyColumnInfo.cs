namespace AdaskoTheBeAsT.ValueSql.SourceGenerator;

public sealed class PropertyColumnInfo
{
    public PropertyColumnInfo(
        string propertyName,
        string propertyType,
        string columnName,
        int? ordinal,
        bool isNullable,
        int length = -2,
        byte precision = 18,
        byte scale = 4,
        string? sqlType = null,
        byte dateTimePrecision = 7)
    {
        PropertyName = propertyName;
        PropertyType = propertyType;
        ColumnName = columnName;
        Ordinal = ordinal;
        IsNullable = isNullable;
        Length = length;
        Precision = precision;
        Scale = scale;
        SqlType = sqlType;
        DateTimePrecision = dateTimePrecision;
    }

    public string PropertyName { get; }

    public string PropertyType { get; }

    public string ColumnName { get; }

    public int? Ordinal { get; }

    public bool IsNullable { get; }

    /// <summary>
    /// Gets the maximum length for string/binary columns. -1 = MAX, -2 = default.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Gets the precision for decimal columns.
    /// </summary>
    public byte Precision { get; }

    /// <summary>
    /// Gets the scale for decimal columns.
    /// </summary>
    public byte Scale { get; }

    /// <summary>
    /// Gets the explicit SQL type override (e.g., "date", "varchar").
    /// </summary>
    public string? SqlType { get; }

    /// <summary>
    /// Gets the precision for datetime2, datetimeoffset, time columns (0-7).
    /// </summary>
    public byte DateTimePrecision { get; }
}
