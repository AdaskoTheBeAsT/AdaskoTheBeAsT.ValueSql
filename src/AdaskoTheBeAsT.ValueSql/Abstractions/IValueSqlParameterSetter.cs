using System.Data;

namespace AdaskoTheBeAsT.ValueSql.Abstractions;

public interface IValueSqlParameterSetter<in T>
{
    void SetParameters(IDbCommand command, T parameters);
}
