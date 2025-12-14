using System;
using System.Data;

namespace AdaskoTheBeAsT.ValueSql.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class ValueSqlParameterAttribute : Attribute
{
    public ValueSqlParameterAttribute()
    {
    }

    public ValueSqlParameterAttribute(string name)
    {
        Name = name;
    }

    public string? Name { get; }

    public DbType? DbType { get; set; }

    public int? Size { get; set; }

    public ParameterDirection Direction { get; set; } = ParameterDirection.Input;
}
