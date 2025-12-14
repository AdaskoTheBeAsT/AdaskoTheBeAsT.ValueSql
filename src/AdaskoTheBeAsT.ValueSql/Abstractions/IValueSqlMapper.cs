using System.Data;

namespace AdaskoTheBeAsT.ValueSql.Abstractions;

public interface IValueSqlMapper<out T>
{
    T Map(IDataReader reader);
}
