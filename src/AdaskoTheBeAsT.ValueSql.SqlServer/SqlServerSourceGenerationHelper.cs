using AdaskoTheBeAsT.ValueSql.SourceGenerator;
using Microsoft.CodeAnalysis;

namespace AdaskoTheBeAsT.ValueSql.SqlServer;

public sealed class SqlServerSourceGenerationHelper
    : SourceGeneratorHelperBase
{
    public SqlServerSourceGenerationHelper()
        : base(new SqlServerMapperClassGenerator())
    {
    }

    protected override void GenerateAdditionalFiles(
        SourceProductionContext context,
        ValueSqlGeneratorOptions options)
    {
    }
}
