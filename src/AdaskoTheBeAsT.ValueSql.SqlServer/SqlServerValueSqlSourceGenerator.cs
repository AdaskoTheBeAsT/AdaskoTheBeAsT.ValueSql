using AdaskoTheBeAsT.ValueSql.SourceGenerator;

namespace AdaskoTheBeAsT.ValueSql.SqlServer;

public sealed class SqlServerValueSqlSourceGenerator
    : ValueSqlSourceGeneratorBase
{
    public SqlServerValueSqlSourceGenerator()
        : base(new SqlServerSourceGenerationHelper())
    {
    }
}
