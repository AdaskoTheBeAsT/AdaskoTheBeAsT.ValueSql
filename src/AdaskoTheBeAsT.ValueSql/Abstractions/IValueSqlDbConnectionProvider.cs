using System.Data;

namespace AdaskoTheBeAsT.ValueSql.Abstractions;

public interface IValueSqlDbConnectionProvider<out TDbConnection>
    where TDbConnection : IDbConnection
{
    TDbConnection Provide();
}
