using System;

namespace AdaskoTheBeAsT.ValueSql.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ValueSqlQueryAttribute : Attribute
{
    public ValueSqlQueryAttribute(string sql)
    {
        Sql = sql;
    }

    public string Sql { get; }
}
